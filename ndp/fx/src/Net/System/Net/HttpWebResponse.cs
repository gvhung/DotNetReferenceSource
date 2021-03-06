//------------------------------------------------------------------------------
// <copyright file="HttpWebResponse.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Net {
    using System.IO;
    using System.IO.Compression;
    using System.Runtime.Serialization;
    using System.ComponentModel;
    using System.Globalization;
    using System.Security.Cryptography.X509Certificates;
    using System.Security.Permissions;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Net.WebSockets;

    //
    // HttpWebResponse - Handles retrival of HTTP Response headers, und Data reads.
    //

    /// <devdoc>
    ///    <para>
    ///    An HTTP-specific implementation of the
    ///    <see cref='System.Net.WebResponse'/> class.
    /// </para>
    /// </devdoc>
    [Serializable]
    public class HttpWebResponse : WebResponse, ISerializable
    {
        // response Uri generated by the request.
        private Uri m_Uri;
        // response Verb gernated by the request
        private KnownHttpVerb m_Verb;
        // response values
        private HttpStatusCode m_StatusCode;
        private string m_StatusDescription;
        // ConnectStream - for reading actual data
        private Stream m_ConnectStream;
        // For the rare case of Post -> Get redirect where we need to re-process the response.
        private CoreResponseData m_CoreResponseData;

        private WebHeaderCollection m_HttpResponseHeaders;

        // Content Length needed for symantics, -1 if chunked
        private long m_ContentLength;

        // for which response ContentType we will look for and parse the CharacterSet
        private string m_MediaType;
        private string m_CharacterSet = null;

        private bool m_IsVersionHttp11;

#if !FEATURE_PAL
        // server certificate for secure connections
        internal X509Certificate m_Certificate;
#endif // !FEATURE_PAL

        private CookieCollection m_cookies;
        private bool m_disposed; // = false;
        private bool m_propertiesDisposed;
        private bool m_UsesProxySemantics;
        private bool m_IsMutuallyAuthenticated;

        private bool m_IsWebSocketResponse;
        private string m_ConnectionGroupName;
        private Stream m_WebSocketConnectionStream = null;

        internal bool IsWebSocketResponse
        {
            get { return m_IsWebSocketResponse; }
            set { m_IsWebSocketResponse = value; }
        }

        internal string ConnectionGroupName
        {
            get { return m_ConnectionGroupName; }
            set { m_ConnectionGroupName = value; }
        }

        //
        // Internal Access to the Response Stream,
        //  public method is GetResponseStream
        //
        internal Stream ResponseStream {
            get {
                return m_ConnectStream;
            }
            set {
                m_ConnectStream = value;
            }
        }

        internal CoreResponseData CoreResponseData
        {
            get { return m_CoreResponseData; }
        }

        //
        public override bool IsMutuallyAuthenticated {
            get {
                CheckDisposed();
                return m_IsMutuallyAuthenticated;
            }
        }
        internal bool InternalSetIsMutuallyAuthenticated {
            set {
                m_IsMutuallyAuthenticated = value;
            }
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public virtual CookieCollection Cookies {
            get {
                CheckDisposed();
                if (m_cookies == null) {
                    m_cookies = new CookieCollection();
                }
                return m_cookies;
            }
            set {
                CheckDisposed();
                m_cookies = value;
            }
        }

        // retreives response header object
        /// <devdoc>
        ///    <para>
        ///       Gets
        ///       the headers associated with this response from the server.
        ///    </para>
        /// </devdoc>
        public override WebHeaderCollection Headers {
            get {
                CheckDisposed();
                return m_HttpResponseHeaders;
            }
        }

        // For portability only
        public override bool SupportsHeaders {
            get {
                return true;
            }
        }

        // ContentLength, -1 indicates unknown value
        /// <devdoc>
        ///    <para>
        ///       Gets the lenght of the content returned by the request.
        ///    </para>
        /// </devdoc>
        public override long ContentLength {
            get {
                CheckDisposed();
                return m_ContentLength;
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Gets the
        ///       method used to encode the body of the response.
        ///    </para>
        /// </devdoc>
        public String ContentEncoding {
            get {
                CheckDisposed();
                string contentEncoding = m_HttpResponseHeaders[HttpKnownHeaderNames.ContentEncoding];
                return contentEncoding == null ? string.Empty : contentEncoding;
            }
        }

        // Returns the Content-Type of the response.

        /// <devdoc>
        ///    <para>
        ///       Gets the content type of the
        ///       response.
        ///    </para>
        /// </devdoc>
        public override string ContentType {
            get {
                CheckDisposed();
                string contentType = m_HttpResponseHeaders.ContentType;
                return contentType == null ? string.Empty : contentType;
            }
        }


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public string CharacterSet {
            get {
                CheckDisposed();
                string contentType = m_HttpResponseHeaders.ContentType;

                if (m_CharacterSet == null && !ValidationHelper.IsBlankString(contentType)) {

                    //sets characterset so the branch is never executed again.
                    m_CharacterSet = String.Empty;

                    //first string is the media type
                    string srchString = contentType.ToLower(CultureInfo.InvariantCulture);

                    //media subtypes of text type has a default as specified by rfc 2616
                    if (srchString.Trim().StartsWith("text/")) {
                        m_CharacterSet = "ISO-8859-1";
                    }

                    //one of the parameters may be the character set
                    //there must be at least a mediatype for this to be valid
                    int i = srchString.IndexOf(";");
                    if (i > 0) {

                        //search the parameters
                        while ((i = srchString.IndexOf("charset",i)) >= 0){

                            i+=7;

                            //make sure the word starts with charset
                            if (srchString[i-8] == ';' || srchString[i-8] == ' '){

                                //skip whitespace
                                while (i < srchString.Length && srchString[i] == ' ')
                                    i++;

                                //only process if next character is '='
                                //and there is a character after that
                                if (i < srchString.Length - 1 && srchString[i] == '=') {
                                   i++;

                                   //get and trim character set substring
                                   int j = srchString.IndexOf(';',i);
                                   //In the past we used
                                   //Substring(i, j). J is the offset not the length
                                   //the qfe is to fix the second parameter so that this it is the
                                   //length. since j points to the next ; the operation j -i
                                   //gives the length of the charset
                                   if (j>i)
                                       m_CharacterSet = contentType.Substring(i,j - i).Trim();
                                   else
                                       m_CharacterSet = contentType.Substring(i).Trim();

                                   //done
                                   break;
                                }
                            }
                        }
                    }
                }
                return m_CharacterSet;
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Gets the name of the server that sent the response.
        ///    </para>
        /// </devdoc>
        public string Server {
            get {
                CheckDisposed();
                string server = m_HttpResponseHeaders.Server;
                return server == null ? string.Empty : server;
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Gets the last
        ///       date and time that the content of the response was modified.
        ///    </para>
        /// </devdoc>
        public  DateTime LastModified {
            get {
                CheckDisposed();

                string lastmodHeaderValue = m_HttpResponseHeaders.LastModified;

                if (lastmodHeaderValue == null) {
                    return DateTime.Now;
                }
                return HttpProtocolUtils.string2date(lastmodHeaderValue);
            }
        }

        // returns StatusCode
        /// <devdoc>
        ///    <para>
        ///       Gets
        ///       a number indicating the status of the response.
        ///    </para>
        /// </devdoc>
        public virtual HttpStatusCode StatusCode {
            get {
                CheckDisposed();
                return m_StatusCode;
            }
        }

        // returns StatusDescription
        /// <devdoc>
        ///    <para>
        ///       Gets the status description returned with the response.
        ///    </para>
        /// </devdoc>
        public virtual string StatusDescription {
            get {
                CheckDisposed();
                return m_StatusDescription;
            }
        }

        // HTTP Version
        /// <devdoc>
        ///    <para>
        ///       Gets
        ///       the version of the HTTP protocol used in the response.
        ///    </para>
        /// </devdoc>
        public Version ProtocolVersion {
            get {
                CheckDisposed();
                return m_IsVersionHttp11 ? HttpVersion.Version11 : HttpVersion.Version10;
            }
        }


        internal bool KeepAlive {
            get {
                //
                // QFE  - DevDiv bug: 37757
                // If there is proxy involved, independen of the Http Version, we should honor the
                // proxy indicated Proxy-Connection header value.
                // This header value is not RFC mandated, but is a legacy from Netscape documentations.
                // It indicates that the proxy wants to keep the connection.
                // Functionally it is equivalent of a Keep-Alive AND/OR Connection header.
                //
                // The absence of this check will result in HTTP/1.0 responsen be considered to be not
                // Keeping the connection alive.
                //
                // This will result in a state mismatch between the connection pool and HttpWebRequest object
                // when the decision to drain the connection and putting it back to the idle pool is made.
                //
                if (m_UsesProxySemantics)
                {
                    string proxyConnectionHeader = Headers[HttpKnownHeaderNames.ProxyConnection];
                    if (proxyConnectionHeader != null)
                    {
                        return proxyConnectionHeader.ToLower(CultureInfo.InvariantCulture).IndexOf("close") < 0 || 
                               proxyConnectionHeader.ToLower(CultureInfo.InvariantCulture).IndexOf("keep-alive") >= 0;
                    }
                }

                string connectionHeader = Headers[HttpKnownHeaderNames.Connection];
                if (connectionHeader != null) 
                {
                    connectionHeader = connectionHeader.ToLower(CultureInfo.InvariantCulture);
                }

                if (ProtocolVersion == HttpVersion.Version10) 
                {
                    // An HTTP/1.0 response is keep-alive if it has a connection: keep-alive header
                    return (connectionHeader != null && 
                            connectionHeader.IndexOf("keep-alive") >= 0);
                }
                else if (ProtocolVersion >= HttpVersion.Version11)
                {
                    // An HTTP/1.1 response is keep-alive if it DOESN'T have a connection: close header, or if it
                    // also has a connection: keep-alive header
                    return (connectionHeader == null ||
                            connectionHeader.IndexOf("close") < 0 ||
                            connectionHeader.IndexOf("keep-alive") >= 0);
                }

                return false;
            }
        }


        /*++

            ResponseStream - Return the response stream.

            This property returns the response stream for this response. The
            response stream will do de-chunking, etc. as needed.

            Input: Nothing. Property is readonly.

            Returns: Response stream for response.

        --*/


        /// <devdoc>
        ///    <para>Gets the stream used for reading the body of the response from the
        ///       server.</para>
        /// </devdoc>
        public override Stream GetResponseStream() {
            if(Logging.On)Logging.Enter(Logging.Web, this, "GetResponseStream", "");
            CheckDisposed();
            if(Logging.On)Logging.PrintInfo(Logging.Web, "ContentLength="+m_ContentLength);

            Stream result;
            if (m_IsWebSocketResponse && m_StatusCode == HttpStatusCode.SwitchingProtocols) // HTTP 101
            {
                if (this.m_WebSocketConnectionStream == null)
                {
                    ConnectStream connectStream = m_ConnectStream as ConnectStream;
                    GlobalLog.Assert(connectStream != null, "HttpWebResponse.m_ConnectStream should always be a ConnectStream in WebSocket cases.");
                    GlobalLog.Assert(connectStream.Connection != null, "HttpWebResponse.m_ConnectStream.Connection should never be null in WebSocket cases.");
                    this.m_WebSocketConnectionStream = new WebSocketConnectionStream(connectStream, this.ConnectionGroupName);
                }
                result = this.m_WebSocketConnectionStream;
            }
            else
            {
                result = m_ConnectStream;
            }
            
            if(Logging.On)Logging.Exit(Logging.Web, this, "GetResponseStream", result);
            return result;
        }

        /*++

            Close - Closes the Response after the use.

            This causes the read stream to be closed.

        --*/

        public override void Close() {
            if(Logging.On)Logging.Enter(Logging.Web, this, "Close", "");
            if (!m_disposed)
            {
                m_disposed = true;
                try
                {
                    Stream stream = m_ConnectStream;
                    ICloseEx icloseEx = stream as ICloseEx;
                    if (icloseEx != null)
                    {
                        icloseEx.CloseEx(CloseExState.Normal /* | CloseExState.Silent */);
                    }
                    else if (stream != null)
                    {
                        stream.Close();
                    }
                }
                finally
                {
                    if (this.IsWebSocketResponse)
                    {
                        ConnectStream connectStream = m_ConnectStream as ConnectStream;
                        if (connectStream != null && connectStream.Connection != null)
                        {
                            connectStream.Connection.ServicePoint.CloseConnectionGroup(ConnectionGroupName);
                        }
                    }
                }
            }
            if(Logging.On)Logging.Exit(Logging.Web, this, "Close", "");
        }

        // This method is only to be called by HttpWebRequest.Abort().
        internal void Abort() {
            Stream stream = m_ConnectStream;
            ICloseEx icloseEx = stream as ICloseEx;
            try
            {
                if (icloseEx != null) {
                    icloseEx.CloseEx(CloseExState.Abort);
                }
                else if (stream != null) {
                    stream.Close();
                }
            }
            catch { }
        }

        [SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", 
            Justification = "The base class calls Close() to clean up m_ConnectStream")]
        protected override void Dispose(bool disposing) {
            if (!disposing) {
                return;
            }
            base.Dispose(true); // Calls Close()
            m_propertiesDisposed = true;
        }

        //introduced for supporting design-time loading of System.Windows.dll
        [Obsolete("This API supports the .NET Framework infrastructure and is not intended to be used directly from your code.", true)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public HttpWebResponse(){}

        internal HttpWebResponse(Uri responseUri, KnownHttpVerb verb, CoreResponseData coreData, string mediaType, 
            bool usesProxySemantics, DecompressionMethods decompressionMethod, 
            bool isWebSocketResponse, string connectionGroupName) {
            m_Uri                       = responseUri;
            m_Verb                      = verb;
            m_MediaType                 = mediaType;
            m_UsesProxySemantics        = usesProxySemantics;

            m_CoreResponseData          = coreData;
            m_ConnectStream             = coreData.m_ConnectStream;
            m_HttpResponseHeaders       = coreData.m_ResponseHeaders;
            m_ContentLength             = coreData.m_ContentLength;
            m_StatusCode                = coreData.m_StatusCode;
            m_StatusDescription         = coreData.m_StatusDescription;
            m_IsVersionHttp11           = coreData.m_IsVersionHttp11;

            m_IsWebSocketResponse       = isWebSocketResponse;
            m_ConnectionGroupName       = connectionGroupName;

            //if the returned contentlength is zero, preemptively invoke calldone on the stream.
            //this will wake up any pending reads.
            if (m_ContentLength == 0 && m_ConnectStream is ConnectStream) {
                ((ConnectStream)m_ConnectStream).CallDone();
            }

            // handle Content-Location header, by combining it with the orginal request.
            string contentLocation = m_HttpResponseHeaders[HttpKnownHeaderNames.ContentLocation];

            if (contentLocation != null) {
                try {
                    m_Uri = new Uri(m_Uri, contentLocation);
                } catch (UriFormatException e) {
                    GlobalLog.Assert("Exception on response Uri parsing.", e.ToString());
                }
            }
            // decompress responses by hooking up a final response Stream - only if user required it
            if(decompressionMethod != DecompressionMethods.None) {
                string contentEncoding = m_HttpResponseHeaders[HttpKnownHeaderNames.ContentEncoding];
                if (contentEncoding != null){
                    if(((decompressionMethod & DecompressionMethods.GZip) != 0) && 
                        contentEncoding.IndexOf(HttpWebRequest.GZipHeader, StringComparison.CurrentCulture) != -1) {
                        m_ConnectStream = new GZipWrapperStream(m_ConnectStream, CompressionMode.Decompress);
                        m_ContentLength = -1; // unknown on compressed streams

                        // Setting a response header after parsing will ruin the Common Header optimization.
                        // This seems like a corner case.  ContentEncoding could be added as a common header, with a special
                        // property allowing it to be nulled.
                        m_HttpResponseHeaders[HttpKnownHeaderNames.ContentEncoding] = null;
                    }
                    else if (((decompressionMethod & DecompressionMethods.Deflate) != 0) && 
                        contentEncoding.IndexOf(HttpWebRequest.DeflateHeader, StringComparison.CurrentCulture) != -1) {
                        m_ConnectStream = new DeflateWrapperStream(m_ConnectStream, CompressionMode.Decompress);
                        m_ContentLength = -1; // unknown on compressed streams

                        // Setting a response header after parsing will ruin the Common Header optimization.
                        // This seems like a corner case.  ContentEncoding could be added as a common header, with a special
                        // property allowing it to be nulled.
                        m_HttpResponseHeaders[HttpKnownHeaderNames.ContentEncoding] = null;
                    }
                }
            }
        }

        //
        // ISerializable constructor
        //
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>

        [Obsolete("Serialization is obsoleted for this type.  http://go.microsoft.com/fwlink/?linkid=14202")]
        protected HttpWebResponse(SerializationInfo serializationInfo, StreamingContext streamingContext):base(serializationInfo, streamingContext) {
            m_HttpResponseHeaders   = (WebHeaderCollection)serializationInfo.GetValue("m_HttpResponseHeaders", typeof(WebHeaderCollection));
            m_Uri                   = (Uri)serializationInfo.GetValue("m_Uri", typeof(Uri));
#if !FEATURE_PAL
            m_Certificate           = (X509Certificate)serializationInfo.GetValue("m_Certificate", typeof(X509Certificate));
#endif // !FEATURE_PAL
            Version version         = (Version)serializationInfo.GetValue("m_Version", typeof(Version));
            m_IsVersionHttp11       = version.Equals(HttpVersion.Version11);
            m_StatusCode            = (HttpStatusCode)serializationInfo.GetInt32("m_StatusCode");
            m_ContentLength         = serializationInfo.GetInt64("m_ContentLength");
            m_Verb                  = KnownHttpVerb.Parse(serializationInfo.GetString("m_Verb"));
            m_StatusDescription     = serializationInfo.GetString("m_StatusDescription");
            m_MediaType             = serializationInfo.GetString("m_MediaType");
        }

        //
        // ISerializable method
        //
        /// <internalonly/>
        [SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase", Justification = "System.dll is still using pre-v4 security model and needs this demand")]
        [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.SerializationFormatter, SerializationFormatter=true)]
        void ISerializable.GetObjectData(SerializationInfo serializationInfo, StreamingContext streamingContext)
        {
            GetObjectData(serializationInfo, streamingContext);
        }

        //
        // FxCop: provide a way for derived classes to access this method even if they reimplement ISerializable.
        //

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter=true)]
        protected override void GetObjectData(SerializationInfo serializationInfo, StreamingContext streamingContext)
        {
            //
            // for now disregard streamingContext.
            // just Add all the members we need to deserialize to construct
            // the object at deserialization time
            //
            // the following runtime types already support serialization:
            // Boolean, Char, SByte, Byte, Int16, UInt16, Int32, UInt32, Int64, UInt64, Single, Double, DateTime
            // for the others we need to provide our own serialization
            //
            serializationInfo.AddValue("m_HttpResponseHeaders", m_HttpResponseHeaders, typeof(WebHeaderCollection));
            serializationInfo.AddValue("m_Uri", m_Uri, typeof(Uri));
#if !FEATURE_PAL
            serializationInfo.AddValue("m_Certificate", m_Certificate, typeof(X509Certificate));
#endif // !FEATURE_PAL
            serializationInfo.AddValue("m_Version", ProtocolVersion, typeof(Version));
            serializationInfo.AddValue("m_StatusCode", m_StatusCode);
            serializationInfo.AddValue("m_ContentLength", m_ContentLength);
            serializationInfo.AddValue("m_Verb", m_Verb.Name);
            serializationInfo.AddValue("m_StatusDescription", m_StatusDescription);
            serializationInfo.AddValue("m_MediaType", m_MediaType);
            base.GetObjectData(serializationInfo, streamingContext);
        }

        /*++

        Routine Description:

            Gets response headers from parsed server response

        Arguments:

            headerName - HTTP header to search for matching header on.

        Return Value:

            string - contains the matched entry, if found

        --*/
        /// <devdoc>
        ///    <para>
        ///       Gets a specified header value returned with the response.
        ///    </para>
        /// </devdoc>
        public string GetResponseHeader( string headerName ) {
            CheckDisposed();

            string headerValue = m_HttpResponseHeaders[headerName];

            return ( (headerValue==null) ? String.Empty : headerValue );
        }

#if HTTP_HEADER_EXTENSIONS_SUPPORTED
        // searches for extension header in response
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public string GetExtension(HttpExtension extension, string header) {
            CheckDisposed();
            return GetResponseHeader(header);
        }
#endif

        /*++

            ResponseUri  - Gets the final Response Uri, that includes any
             changes that may have transpired from the orginal Request

            This property returns Uri for this WebResponse.

            Input: Nothing.

            Returns: Response Uri for response.

                    read-only

        --*/

        /// <devdoc>
        ///    <para>
        ///       Gets the Uniform Resource Indentifier (Uri) of the resource that returned the
        ///       response.
        ///    </para>
        /// </devdoc>
        public override Uri ResponseUri {                               // read-only
            get {
                CheckDisposed();
                return m_Uri;
            }
        }

        /*
            Accessor:   Method

            Gets/Sets the http method of this request.
            This method represents the Verb,
            after any redirects

            Returns: Method currently being used.


        */
        /// <devdoc>
        ///    <para>
        ///       Gets the value of the method used to return the response.
        ///    </para>
        /// </devdoc>
        public virtual string Method {
            get {
                CheckDisposed();
                return m_Verb.Name;
            }
        }

        private void CheckDisposed() {
            if (m_propertiesDisposed) {
                throw new ObjectDisposedException(this.GetType().FullName);
            }
        }
    } // class HttpWebResponse

    // transfer ICloseEx behavior to nested compound stream.
    internal class GZipWrapperStream : GZipStream, ICloseEx, IRequestLifetimeTracker {

        public GZipWrapperStream(Stream stream, CompressionMode mode) : base( stream, mode, false) {
        }

        void ICloseEx.CloseEx(CloseExState closeState) {
            ICloseEx icloseEx = BaseStream as ICloseEx;
            if (icloseEx != null) {
                // since the internal Close() of GZipStream just does Close on our base stream
                // we don't have to call it anymore at this point
                icloseEx.CloseEx(closeState);
            }
            else {
                Close();
            }
        }

        public override IAsyncResult BeginRead(byte[] buffer, int offset, int size, AsyncCallback callback, Object state) {

            if (buffer==null) {
                throw new ArgumentNullException("buffer");
            }
            if (offset<0 || offset>buffer.Length) {
                throw new ArgumentOutOfRangeException("offset");
            }
            if (size<0 || size>buffer.Length-offset) {
                throw new ArgumentOutOfRangeException("size");
            }

            try{
                return base.BeginRead(buffer, offset, size, callback, state);
            }
            catch(Exception e){
                try{
                    if (NclUtilities.IsFatal(e)) throw;
                    if(e is System.IO.InvalidDataException || e is InvalidOperationException || e is IndexOutOfRangeException){
                        Close();
                    }
                }
                catch{
                }
                throw e;
            }
        }

        public override int EndRead(IAsyncResult asyncResult) {

            if (asyncResult==null) {
                throw new ArgumentNullException("asyncResult");
            }

            try{
                return base.EndRead(asyncResult);
            }
            catch(Exception e){
                try{
                    if (NclUtilities.IsFatal(e)) throw;
                    if(e is System.IO.InvalidDataException || e is InvalidOperationException || e is IndexOutOfRangeException){
                        Close();
                    }
                }
                catch{
                }
                throw e;
            }
        }

        public override int Read(byte[] buffer, int offset, int size) {

            if (buffer==null) {
                throw new ArgumentNullException("buffer");
            }
            if (offset<0 || offset>buffer.Length) {
                throw new ArgumentOutOfRangeException("offset");
            }
            if (size<0 || size>buffer.Length-offset) {
                throw new ArgumentOutOfRangeException("size");
            }

            try{
                return base.Read(buffer, offset, size);
            }
            catch(Exception e){
                try{
                    if (NclUtilities.IsFatal(e)) throw;

                    if(e is System.IO.InvalidDataException || e is InvalidOperationException || e is IndexOutOfRangeException){
                        Close();
                    }
                }
                catch{
                }
                throw e;
            }
        }

        void IRequestLifetimeTracker.TrackRequestLifetime(long requestStartTimestamp)
        {
            IRequestLifetimeTracker stream = BaseStream as IRequestLifetimeTracker;
            Debug.Assert(stream != null, "Wrapped stream must implement IRequestLifetimeTracker interface");
            stream.TrackRequestLifetime(requestStartTimestamp);
        }
    }


    internal class DeflateWrapperStream : DeflateStream, ICloseEx, IRequestLifetimeTracker {

        public DeflateWrapperStream(Stream stream, CompressionMode mode) : base( stream, mode, false) {
        }

        void ICloseEx.CloseEx(CloseExState closeState) {
            ICloseEx icloseEx = BaseStream as ICloseEx;
            if (icloseEx != null) {
                // since the internal Close() of GZipStream just does Close on our base stream
                // we don't have to call it anymore at this point
                icloseEx.CloseEx(closeState);
            }
            else {
                Close();
            }
        }


        public override IAsyncResult BeginRead(byte[] buffer, int offset, int size, AsyncCallback callback, Object state) {

            if (buffer==null) {
                throw new ArgumentNullException("buffer");
            }
            if (offset<0 || offset>buffer.Length) {
                throw new ArgumentOutOfRangeException("offset");
            }
            if (size<0 || size>buffer.Length-offset) {
                throw new ArgumentOutOfRangeException("size");
            }

            try{
                return base.BeginRead(buffer, offset, size, callback, state);
            }
            catch(Exception e){
                try{
                    if (NclUtilities.IsFatal(e)) throw;
                    if(e is System.IO.InvalidDataException || e is InvalidOperationException || e is IndexOutOfRangeException){
                        Close();
                    }
                }
                catch{
                }
                throw e;
            }
        }

        public override int EndRead(IAsyncResult asyncResult) {

            if (asyncResult==null) {
                throw new ArgumentNullException("asyncResult");
            }

            try{
                return base.EndRead(asyncResult);
            }
            catch(Exception e){
                try{
                    if (NclUtilities.IsFatal(e)) throw;
                    if(e is System.IO.InvalidDataException || e is InvalidOperationException || e is IndexOutOfRangeException){
                        Close();
                    }
                }
                catch{
                }
                throw e;
            }
        }

        public override int Read(byte[] buffer, int offset, int size) {

            if (buffer==null) {
                throw new ArgumentNullException("buffer");
            }
            if (offset<0 || offset>buffer.Length) {
                throw new ArgumentOutOfRangeException("offset");
            }
            if (size<0 || size>buffer.Length-offset) {
                throw new ArgumentOutOfRangeException("size");
            }

            try{
                return base.Read(buffer, offset, size);
            }
            catch(Exception e){
                try{
                    if (NclUtilities.IsFatal(e)) throw;
                    if(e is System.IO.InvalidDataException || e is InvalidOperationException || e is IndexOutOfRangeException){
                        Close();
                    }
                }
                catch{
                }
                throw e;
            }
        }

        void IRequestLifetimeTracker.TrackRequestLifetime(long requestStartTimestamp)
        {
            IRequestLifetimeTracker stream = BaseStream as IRequestLifetimeTracker;
            Debug.Assert(stream != null, "Wrapped stream must implement IRequestLifetimeTracker interface");
            stream.TrackRequestLifetime(requestStartTimestamp);
        }
    }



} // namespace System.Net
