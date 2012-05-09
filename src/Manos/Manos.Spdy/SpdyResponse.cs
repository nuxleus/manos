using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Manos.Http;
using Manos.IO;

namespace Manos.Spdy
{
    public class SpdyResponse : SpdyEntity, IHttpResponse
    {
        private readonly Dictionary<string, HttpCookie> cookies;
        private readonly SpdyStream writestream;
        private Dictionary<string, object> properties;

        public SpdyResponse(SpdyRequest req, SpdyStream writestream, Context context)
            : base(context)
        {
            Request = req;
            Headers = new HttpHeaders();
            cookies = new Dictionary<string, HttpCookie>();
            this.writestream = writestream;
        }

        public SpdyRequest Request { get; set; }

        #region IHttpResponse Members

        public HttpStream Stream
        {
            get { throw new NotImplementedException("Stream"); }
        }


        public StreamWriter Writer
        {
            get { throw new NotImplementedException("Writer"); }
        }

        #endregion

        #region IHttpResponse implementation

        public void Complete(Action callback)
        {
            EnsureReplyWritten();
            writestream.End();
            callback();
        }

        public void SendFile(string file)
        {
            Headers.SetNormalizedHeader("Content-Type", ManosMimeTypes.GetMimeType(file));
            EnsureReplyWritten();
            writestream.SendFile(file);
        }

        public void Redirect(string url)
        {
            Headers.SetNormalizedHeader("Location", url);
            StatusCode = 302;
            EnsureReplyWritten(true);
            End();
        }

        public void SetHeader(string name, string value)
        {
            Headers.SetHeader(name, value);
        }

        public void SetCookie(string name, HttpCookie cookie)
        {
            cookies[name] = cookie;
        }

        public HttpCookie SetCookie(string name, string value)
        {
            if (name == null)
                throw new ArgumentNullException("name");
            if (value == null)
                throw new ArgumentNullException("value");

            var cookie = new HttpCookie(name, value);

            SetCookie(name, cookie);
            return cookie;
        }

        public HttpCookie SetCookie(string name, string value, string domain)
        {
            if (name == null)
                throw new ArgumentNullException("name");
            if (value == null)
                throw new ArgumentNullException("value");

            var cookie = new HttpCookie(name, value);
            cookie.Domain = domain;

            SetCookie(name, cookie);
            return cookie;
        }

        public HttpCookie SetCookie(string name, string value, DateTime expires)
        {
            return SetCookie(name, value, null, expires);
        }

        public HttpCookie SetCookie(string name, string value, string domain, DateTime expires)
        {
            if (name == null)
                throw new ArgumentNullException("name");
            if (value == null)
                throw new ArgumentNullException("value");

            var cookie = new HttpCookie(name, value);

            cookie.Domain = domain;
            cookie.Expires = expires;

            SetCookie(name, cookie);
            return cookie;
        }

        public HttpCookie SetCookie(string name, string value, TimeSpan max_age)
        {
            return SetCookie(name, value, DateTime.Now + max_age);
        }

        public HttpCookie SetCookie(string name, string value, string domain, TimeSpan max_age)
        {
            return SetCookie(name, value, domain, DateTime.Now + max_age);
        }

        public void RemoveCookie(string name)
        {
            var cookie = new HttpCookie(name, "");
            cookie.Expires = DateTime.Now.AddYears(-1);

            SetCookie(name, cookie);
        }

        public void Read()
        {
            throw new NotImplementedException("Read");
        }

        public void WriteMetadata(StringBuilder builder)
        {
            throw new NotImplementedException("WriteMetadata");
        }

        public int StatusCode { get; set; }

        public bool WriteHeaders { get; set; }

        private void EnsureReplyWritten(bool done)
        {
            if (writestream.ReplyWritten)
                return;
            writestream.WriteReply(this, done);
        }

        private void EnsureReplyWritten()
        {
            EnsureReplyWritten(false);
        }


        public override void WriteToBody(byte[] data, int offset, int length)
        {
            EnsureReplyWritten();
            writestream.Write(data, offset, length);
        }

        #endregion
    }
}