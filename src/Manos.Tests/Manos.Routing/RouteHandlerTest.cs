//
// Copyright (C) 2010 Jackson Harper (jackson@manosdemono.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
//


using System;
using Manos.Http;
using Manos.Http.Testing;
using Manos.Routing.Testing;
using Manos.ShouldExt;
using NUnit.Framework;

namespace Manos.Routing.Tests
{
    [TestFixture]
    public class RouteHandlerTest
    {
        public class TestApp : ManosApp
        {
            public TestApp()
            {
                Route("/TESTING", new TestModule());
            }

            #region Nested type: TestModule

            public class TestModule : ManosModule
            {
                public void Route1(IManosContext ctx)
                {
                    ctx.Response.Write("Route1");
                    ctx.Response.End();
                }

                [Get("/Route2a/{age}/{name}")]
                public void Route2a(TestApp app, IManosContext ctx, String name, int age)
                {
                    ctx.Response.Write("(R2a) Hello '{0}', you are '{1}'", name, age);
                    ctx.Response.End();
                }

                [Get("/Route2b/{name}/{age}")]
                public void Route2b(TestApp app, IManosContext ctx, String name, int age)
                {
                    ctx.Response.Write("(R2b) Hello '{0}', you are '{1}'", name, age);
                    ctx.Response.End();
                }

                [Get("/Route3/(?<name>.+?)/(?<age>.+?)")]
                public void Route3(TestApp app, IManosContext ctx, String name, int age)
                {
                    ctx.Response.Write("'{0}', you are '{1}'", name, age);
                    ctx.Response.End();
                }
            }

            #endregion
        }

        private static void FakeAction(IManosContext ctx)
        {
        }

        private static void FakeAction2(IManosContext ctx)
        {
        }

        [Test]
        public void Find_PartialMatchAtBeginningOfChildlessHandler_ReturnsProperRoute()
        {
            IMatchOperation fooOp = MatchOperationFactory.Create("foo", MatchType.String);
            IMatchOperation foobarOp = MatchOperationFactory.Create("foobar", MatchType.String);
            var rh_bad = new RouteHandler(fooOp, HttpMethod.HTTP_GET, new ActionTarget(FakeAction));
            var rh_good = new RouteHandler(foobarOp, HttpMethod.HTTP_GET, new ActionTarget(FakeAction2));
            var rh = new RouteHandler();

            rh.Add(rh_bad);
            rh.Add(rh_good);


            var request = new MockHttpRequest(HttpMethod.HTTP_GET, "foobar");
            IManosTarget res = rh.Find(request);

            Assert.AreEqual(rh_good.Target, res);
        }

        [Test]
        public void Find_PartialMatchAtBeginningOfHandlerWithChildren_ReturnsProperRoute()
        {
            IMatchOperation fooOp = MatchOperationFactory.Create("foo", MatchType.String);
            IMatchOperation foobarOp = MatchOperationFactory.Create("foobar", MatchType.String);
            IMatchOperation blahOp = MatchOperationFactory.Create("blah", MatchType.String);
            var rh_bad = new RouteHandler(fooOp, HttpMethod.HTTP_GET);
            var rh_good = new RouteHandler(foobarOp, HttpMethod.HTTP_GET, new ActionTarget(FakeAction2));
            var rh = new RouteHandler();

            rh_bad.Children.Add(new RouteHandler(blahOp, HttpMethod.HTTP_GET, new ActionTarget(FakeAction)));

            rh.Add(rh_bad);
            rh.Add(rh_good);

            var request = new MockHttpRequest(HttpMethod.HTTP_GET, "foobar");
            IManosTarget res = rh.Find(request);

            Assert.AreEqual(rh_good.Target, res);
        }

        [Test]
        public void HasPatternsTest()
        {
            IMatchOperation fooOp = MatchOperationFactory.Create("foo/", MatchType.String);
            IMatchOperation foobarOp = MatchOperationFactory.Create("foobar", MatchType.String);
            var rh = new RouteHandler(fooOp, HttpMethod.HTTP_GET);

            Assert.IsTrue(rh.HasPatterns, "a1");

            rh.Children.Clear();
            Assert.IsFalse(rh.HasPatterns, "a2");

            rh.Add(new RouteHandler(foobarOp, HttpMethod.HTTP_GET));
            Assert.IsTrue(rh.HasPatterns, "a3");

            rh.MatchOps = null;
            Assert.IsFalse(rh.HasPatterns, "a4");
        }

        [Test]
        public void ImplicitRouteWorksWithModuleOnCustomApp()
        {
            var t = new TestApp();
            var req = new MockHttpRequest(HttpMethod.HTTP_GET, "/TESTING/Route1");
            var res = new MockHttpResponse();
            var txn = new MockHttpTransaction(req, res);
            t.HandleTransaction(t, txn);

            Assert.AreEqual("Route1", txn.ResponseString);

            var t2 = new TestApp();
            var req2 = new MockHttpRequest(HttpMethod.HTTP_GET, "/TESTING/Route1/");
            var res2 = new MockHttpResponse();
            var txn2 = new MockHttpTransaction(req2, res2);
            t2.HandleTransaction(t2, txn2);

            Assert.AreEqual("Route1", txn2.ResponseString);
        }

        [Test]
        public void RouteWorksWithNamedParametersInModuleOnCustomApp()
        {
            var t = new TestApp();
            var req = new MockHttpRequest(HttpMethod.HTTP_GET, "/TESTING/Route2a/29/Andrew");
            var res = new MockHttpResponse();
            var txn = new MockHttpTransaction(req, res);
            t.HandleTransaction(t, txn);

            Assert.AreEqual("(R2a) Hello 'Andrew', you are '29'", txn.ResponseString);

            var t2 = new TestApp();
            var req2 = new MockHttpRequest(HttpMethod.HTTP_GET, "/TESTING/Route2b/Andrew/29");
            var res2 = new MockHttpResponse();
            var txn2 = new MockHttpTransaction(req2, res2);
            t2.HandleTransaction(t2, txn2);

            Assert.AreEqual("(R2b) Hello 'Andrew', you are '29'", txn2.ResponseString);
        }

        [Test]
        public void RouteWorksWithRegexParamsInModuleOnCustomApp()
        {
            var t = new TestApp();
            var req = new MockHttpRequest(HttpMethod.HTTP_GET, "/TESTING/Route3/Andrew/29");
            var res = new MockHttpResponse();
            var txn = new MockHttpTransaction(req, res);
            t.HandleTransaction(t, txn);

            Assert.AreEqual("'Andrew', you are '29'", txn.ResponseString);

            var t2 = new TestApp();
            var req2 = new MockHttpRequest(HttpMethod.HTTP_GET, "/TESTING/Route3/Andrew/29/");
            var res2 = new MockHttpResponse();
            var txn2 = new MockHttpTransaction(req2, res2);
            t2.HandleTransaction(t2, txn2);

            Assert.AreEqual("'Andrew', you are '29'", txn2.ResponseString);
        }


        [Test]
        public void TestChangePatterns()
        {
            //
            // Ensure that changing the patterns property works.
            // This is a bit of an edge case because internally
            // the patterns strings are cached as an array of 
            // regexes.
            //
            IMatchOperation fooOp = MatchOperationFactory.Create("^foo", MatchType.Regex);
            var target = new MockManosTarget();
            var rh = new RouteHandler(fooOp, HttpMethod.HTTP_GET, target);
            var request = new MockHttpRequest(HttpMethod.HTTP_GET, "foo");

            rh.MatchOps[0] = MatchOperationFactory.Create("bar", MatchType.Regex);
            ;
            Assert.AreEqual(target, rh.Find(request), "sanity-1");


            Assert.IsNull(rh.Find(request), "sanity-2");

            request = new MockHttpRequest(HttpMethod.HTTP_GET, "baz");
            Assert.AreEqual(target, rh.Find(request), "changed");
        }

        [Test]
        public void TestNoChildrenOfTarget()
        {
            IMatchOperation fooOp = MatchOperationFactory.Create("foo", MatchType.String);
            var rh = new RouteHandler(fooOp, HttpMethod.HTTP_GET, new ActionTarget(FakeAction));

            Should.Throw<InvalidOperationException>(() => rh.Children.Add(new RouteHandler(fooOp, HttpMethod.HTTP_POST)));
        }

        [Test]
        public void TestSetPatternsNull()
        {
            IMatchOperation fooOp = MatchOperationFactory.Create("^foo", MatchType.Regex);
            IMatchOperation bazOp = MatchOperationFactory.Create("baz", MatchType.Regex);

            var target = new MockManosTarget();
            var rh = new RouteHandler(fooOp, HttpMethod.HTTP_GET, target);
            var bazRh = new RouteHandler(bazOp, HttpMethod.HTTP_GET, target);
            var request = new MockHttpRequest(HttpMethod.HTTP_GET, "foo");

            Assert.AreEqual(target, rh.Find(request), "sanity-1");

            rh.Children[0] = bazRh;
            Assert.IsNull(rh.Find(request), "sanity-2");

            request = new MockHttpRequest(HttpMethod.HTTP_GET, "baz");
            Assert.AreEqual(target, rh.Find(request), "changed");
        }

        [Test]
        public void TestStrMatch()
        {
            IMatchOperation op = MatchOperationFactory.Create("^foo", MatchType.Regex);
            var target = new MockManosTarget();
            var rh = new RouteHandler(op, HttpMethod.HTTP_GET, target);
            var request = new MockHttpRequest(HttpMethod.HTTP_GET, "foo");

            Assert.AreEqual(target, rh.Find(request), "should-match");

            request = new MockHttpRequest(HttpMethod.HTTP_GET, "garbage-foo");
            Assert.IsNull(rh.Find(request), "garbage-input");
        }

        [Test]
        public void TestStrMatchDeep()
        {
            IMatchOperation fooOp = MatchOperationFactory.Create("foo/", MatchType.String);
            IMatchOperation barOp = MatchOperationFactory.Create("bar", MatchType.String);
            var target = new MockManosTarget();
            var rh = new RouteHandler(fooOp, HttpMethod.HTTP_GET)
                         {
                             new RouteHandler(barOp, HttpMethod.HTTP_GET, target),
                         };

            var request = new MockHttpRequest(HttpMethod.HTTP_GET, "foo/bar");
            Assert.AreEqual(target, rh.Find(request));

            request = new MockHttpRequest(HttpMethod.HTTP_GET, "foo/foo");
            Assert.IsNull(rh.Find(request), "repeate-input");

            request = new MockHttpRequest(HttpMethod.HTTP_GET, "foo/badbar");
            Assert.IsNull(rh.Find(request), "matched-input");
        }

        [Test]
        public void UriParamsTest()
        {
            IMatchOperation fooOp = MatchOperationFactory.Create("(?<name>.+)", MatchType.Regex);
            var rh = new RouteHandler(fooOp, HttpMethod.HTTP_GET, new ActionTarget(FakeAction));
            var request = new MockHttpRequest(HttpMethod.HTTP_GET, "hello");

            Should.NotBeNull(rh.Find(request), "target");

            Should.NotBeNull(request.UriData, "uri-data");

            Assert.AreEqual("hello", request.UriData["name"]);
        }

        [Test]
        public void UriParamsTestDeep()
        {
            IMatchOperation animalOp = MatchOperationFactory.Create("(?<animal>.+)/", MatchType.Regex);
            IMatchOperation nameOp = MatchOperationFactory.Create("(?<name>.+)/", MatchType.Regex);
            var rh = new RouteHandler(animalOp, HttpMethod.HTTP_GET)
                         {
                             new RouteHandler(nameOp, HttpMethod.HTTP_GET, new ActionTarget(FakeAction)),
                         };
            var request = new MockHttpRequest(HttpMethod.HTTP_GET, "dog/roxy");

            Should.NotBeNull(rh.Find(request), "target");

            Should.NotBeNull(request.UriData, "uri-data");

            Assert.AreEqual("dog", request.UriData["animal"]);
            Assert.AreEqual("roxy", request.UriData["name"]);
        }
    }
}