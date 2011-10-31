using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading;
using Filmster.Common;
using Filmster.Crawler;
using Filmster.Data;
using HtmlAgilityPack;

namespace Filmster.Crawlers
{
    internal class FilmstribenCrawler : Crawler
    {
        private const string _crawlstart = "http://www.filmstriben.dk/fjernleje/film/search.aspx";

        public void Start()
        {
            var page = 0;
            var resultContainsMovies = true;
            var moviesToLoad = new List<string>();

            while (resultContainsMovies)
                //while (page == 0)
            {
                Logger.LogVerbose("Fetching Filmstriben page " + page + ": " + string.Format(_crawlstart, page * 24));
                var postData = new NameValueCollection()
                                   {
                                       {"__EVENTTARGET", "ctl00$MainContent$PagingList$ctl02$pagingbutton"},
                                       {"__EVENTVALIDATION", "/wEWVwLaqeqoBALAh92ZAwK07NioCwKh0KL2CQLF4vqGDgKdjbzuBAKlwdeKCQK6wdeKCQK7wdeKCQK4wdeKCQK5wdeKCQK6wauJCQK+wdeKCQK/wdeKCQK8wdeKCQKtwdeKCQKiwdeKCQK6wa+JCQK6wbOJCQK6wZuJCQK0oqOVCgKroqOVCgKqoqOVCgKpoqOVCgKtoqOVCgLOrIy7DAKa1KHICgKBw4PnBAKkquSSDgLLkMaJCALu/9ikAgKV5rrSDQK4zZzJBwKPp9yIDgKyjr6mCAL27etSAp3UzckKAq/Q/NcHAuaRwLUDAsfO2aANAsbO0eELAsXOyeULAsTOwZ8KAsrO+YgHAqbF26ANAqXF0+ELAqTFy+ULAqvFw58KAqnF+4gHAvXA5aANAvTA3eILAvvA1eULAvrAzZ8KAvjAhYkHAtS336ANAtu31+ELAtq3z+ULAtm3x58KAs+3/4gHAruz0p8NArqzyuELArmzwuULArizup8KAq6z8ogHApqq1J8NApmqzOELApiqxOULAo+qvJ8KAo2q9IgHArnjyp8NArjjwuELAq/juuULAq7jsp8KAqzj6ogHApia3KANAo+a1OELAo6azOULAo2axJ8KApOa/IgHAr+I4KQNAr6I+N4LAr2I0OkLAryI6OIKAsKIgI0HAp7/2aQNAp3/8d4LAq3ItbYChRSDV7vFWQElwegwcxpZG/fuVL8="},
                                       {"__VIEWSTATE", "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YWYgZGVuIDMwIMOlciB5bmdyZSBkb2t1bWVudGFyaXN0IE1hcnRpbi4gQWx0IGltZW5zIEFuZHLDqSBmb3J0w6ZsbGVyIHRpbCBNYXJ0aW5zIGthbWVyYS4uLgYzMG1pbiAoPGEgaHJlZj0iY2F0YWxvZy5hc3B4P2dlbnJlPTIiPkRyYW1hPC9hPmQCAQ9kFgJmDxUKCjkwMDAwMDAwOTYKOTAwMDAwMDA5Ng9BZnRhbGVyIG1lZCBHdWQPQWZ0YWxlciBtZWQgR3VkZjxpbWcgc3JjPSIvZmplcm5sZWplL2NvbnRlbnQvaW1nL21lZGllcmFhZGV0L2EucG5nIiBhbHQ9IlRpbGxhZHQgZm9yIGFsbGUiIHRpdGxlPSJUaWxsYWR0IGZvciBhbGxlIiAvPgo5MDAwMDAwMDk2D0FmdGFsZXIgbWVkIEd1ZLQBRmlsbWVuIGVyIGV0IGVzc2F5IG9tIHRyb2VuIG9nIGLDuG5uZW4gZm9ydGFsdCBnZW5uZW0gZGVuIGRhbnNrZSBub3ZpY2UgQW50b25pYSBIb2xzdGVpbi1MZWRyZWJvcmcgcMOlIGhlbmRlcyBmb3J0c2F0dGUgdmVqIHRpbCBtw7hkZXQgbWVkIEd1ZC4gU29tIGthdG9sc2sgb3BkcmFnZXQgcMOlIGVuIGRhbnNrLi4uBjI2bWluIABkAgIPZBYCZg8VCgoyODgzMjgzNDAwCjI4ODMyODM0MDAHQWdub3NpYQdBZ25vc2lhgQE8aW1nIHNyYz0iL2ZqZXJubGVqZS9jb250ZW50L2ltZy9tZWRpZXJhYWRldC8xNS5wbmciIGFsdD0iVGlsbGFkdCBmb3IgYsO4cm4gb3ZlciAxNSDDpXIiIHRpdGxlPSJUaWxsYWR0IGZvciBiw7hybiBvdmVyIDE1IMOlciIgLz4KMjg4MzI4MzQwMAdBZ25vc2lhtgFKb2FuYSBsaWRlciBhZiBkZW4gc2rDpmxkbmUgc3lnZG9tIEFnbm9zaWEsIHNvbSBnw7hyLCBhdCBoZW5kZXMgaGplcm5lIGlra2Uga2FuIGFma29kZSwgaHZhZCBoZW5kZXMgw7hqbmUgb2cgw7hyZXIgb3BmYXR0ZXIuIE1lbiBKb2FuYXMgaG92ZWQgaW5kZWhvbGRlciBtZXJlIGVuZCBkZW4gw7hkZWzDpmdnZW5kZS4uLgkxdCA1MG1pbiBWPGEgaHJlZj0iY2F0YWxvZy5hc3B4P2dlbnJlPTIiPkRyYW1hPC9hPiAvIDxhIGhyZWY9ImNhdGFsb2cuYXNweD9nZW5yZT04Ij5UaHJpbGxlcjwvYT5kAgMPZBYCZg8VCgoyODQxNzcyMTAwCjI4NDE3NzIxMDAFQWdvcmEFQWdvcmGBATxpbWcgc3JjPSIvZmplcm5sZWplL2NvbnRlbnQvaW1nL21lZGllcmFhZGV0LzE1LnBuZyIgYWx0PSJUaWxsYWR0IGZvciBiw7hybiBvdmVyIDE1IMOlciIgdGl0bGU9IlRpbGxhZHQgZm9yIGLDuHJuIG92ZXIgMTUgw6VyIiAvPgoyODQxNzcyMTAwBUFnb3JhtQFFZ3lwdGVuLCBSb21lcnJpZ2V0cyBBbGV4YW5kcmlhLCDDpXIgMzAwLiBEZW4ga3ZpbmRlbGlnZSBmaWxvc29mIEh5cGF0aWEgdW5kZXJ2aXNlciBww6UgYmlibGlvdGVrZXQuIERldCBzdG9yZSBzcMO4cmdzbcOlbCBmb3IgSHlwYXRpYSBvZyBoZW5kZXMgZWxldmVyIGVyIHNvbHN5c3RlbWV0cyBvcGJ5Z25pbmcsLi4uCDJ0IDFtaW4gKDxhIGhyZWY9ImNhdGFsb2cuYXNweD9nZW5yZT0yIj5EcmFtYTwvYT5kAgQPZBYCZg8VCgoyNzU3MTkyMTAwCjI3NTcxOTIxMDAZQWlyIEJ1ZCAtIGdvbGRlbiByZWNlaXZlchlBaXIgQnVkIC0gZ29sZGVuIHJlY2VpdmVyZjxpbWcgc3JjPSIvZmplcm5sZWplL2NvbnRlbnQvaW1nL21lZGllcmFhZGV0L2EucG5nIiBhbHQ9IlRpbGxhZHQgZm9yIGFsbGUiIHRpdGxlPSJUaWxsYWR0IGZvciBhbGxlIiAvPgoyNzU3MTkyMTAwGUFpciBCdWQgLSBnb2xkZW4gcmVjZWl2ZXKyAUJ1ZGR5IGVyIGVuIGdvbGRlbiByZXRyaWV2ZXIsIHNvbSBib3IgaG9zIEpvc2gsIEFuZHJlYSBvZyBkZXJlcyBtb3IuIEJ1ZGR5IGVyIGdvZCB0aWwgYmFza2V0YmFsbC4gUnVzc2lza2UgY2lya3VzZm9sayBoYXIgZsOlZXQgw7hqZSBww6UgQnVkZHlzIHRhbGVudCBmb3IgYm9sZHNwaWwsIG9nIGRhIEpvc2guLi4JMXQgMzBtaW4gajxhIGhyZWY9ImNhdGFsb2cuYXNweD9nZW5yZT00Ij5Lb21lZGllPC9hPiAvIDxhIGhyZWY9ImNhdGFsb2cuYXNweD9nZW5yZT0xNyI+Rm9yIHN0JiMyNDg7cnJlIGImIzI0ODtybjwvYT5kAgUPZBYCZg8VCgoyNTk4MDEwNzAwCjI1OTgwMTA3MDAMQWxkZXJkb20gMS03DEFsZGVyZG9tIDEtN4EBPGltZyBzcmM9Ii9mamVybmxlamUvY29udGVudC9pbWcvbWVkaWVyYWFkZXQvMTUucG5nIiBhbHQ9IlRpbGxhZHQgZm9yIGLDuHJuIG92ZXIgMTUgw6VyIiB0aXRsZT0iVGlsbGFkdCBmb3IgYsO4cm4gb3ZlciAxNSDDpXIiIC8+CjI1OTgwMTA3MDAMQWxkZXJkb20gMS03sQFTeXYgdW5kZXJmdW5kaWdlIGZpbG0gb20gYXQgYmxpdmUgZ2FtbWVsLiBEZXIgc8OmdHRlcyBmb2t1cyBww6Ugw6ZsZHJlIG1lbm5lc2tlciBpIERhbm1hcmsgb2cgZGVyZXMgdW5pa2tlIGJhZ2FnZSBhZiBlcmZhcmluZ2VyLCBkZXIgbGlnZ2VyIHRpbCBncnVuZCBmb3IgZGUgY2VudHJhbGUgdsOmcmRpZXIuLi4JMXQgMThtaW4gAGQCBg9kFgJmDxUKCjI4Mjg1MTQwMDAKMjgyODUxNDAwMBZBbGljZSBCYWJzIC0gU3dpbmcgaXQhFkFsaWNlIEJhYnMgLSBTd2luZyBpdCFmPGltZyBzcmM9Ii9mamVybmxlamUvY29udGVudC9pbWcvbWVkaWVyYWFkZXQvYS5wbmciIGFsdD0iVGlsbGFkdCBmb3IgYWxsZSIgdGl0bGU9IlRpbGxhZHQgZm9yIGFsbGUiIC8+CjI4Mjg1MTQwMDAWQWxpY2UgQmFicyAtIFN3aW5nIGl0IbUBJ0FsaWNlIEJhYnMsIFN3aW5nIEl0JyBlciBmaWxtZW4gb20gU3ZlcmlnZXMgc3RvcmUgaW50ZXJuYXRpb25hbGUgamF6ei1zYW5nZXJpbmRlIEFsaWNlIEJhYnMuIEZyYSBmYXR0aWdlIGZvcmhvbGQga29tIGh1biB0aWwgU3RvY2tob2xtIG9nIGJsZXYgaHVydGlndCBzdGplcm5lLiBQcsOmc3RlciBhZnNreWVkZS4uLgkxdCAyMG1pbiAoPGEgaHJlZj0iY2F0YWxvZy5hc3B4P2dlbnJlPTYiPk11c2lrPC9hPmQCBw9kFgJmDxUKCjIyMjI0MTc0MDAKMjIyMjQxNzQwMBJBbGlnZXJtYWFzIGV2ZW50eXISQWxpZ2VybWFhcyBldmVudHlyZjxpbWcgc3JjPSIvZmplcm5sZWplL2NvbnRlbnQvaW1nL21lZGllcmFhZGV0L2EucG5nIiBhbHQ9IlRpbGxhZHQgZm9yIGFsbGUiIHRpdGxlPSJUaWxsYWR0IGZvciBhbGxlIiAvPgoyMjIyNDE3NDAwEkFsaWdlcm1hYXMgZXZlbnR5crMBQWxpZ2VybWFhIHDDpSBvdHRlIMOlciBzaWRkZXIgcMOlIGVuIGhlc3QsIHNvbSBlciBodW4gZsO4ZHQgcMOlIGRlbi4gSHVuIGJvciBzYW1tZW4gbWVkIGZhciBvZyBtb3IsIHN0b3JlLSBvZyBsaWxsZWJyb3IgaSBNb25nb2xpZXQuIFDDpSBkZSBzdG9yZSBzdGVwcGVyLCBodm9yIHZpbmRlbiBzdXNlciwgb2cuLi4IMXQgNG1pbiCaATxhIGhyZWY9ImNhdGFsb2cuYXNweD9nZW5yZT0yIj5EcmFtYTwvYT4gLyA8YSBocmVmPSJjYXRhbG9nLmFzcHg/Z2VucmU9OSI+VW5kZXJ2aXNuaW5nPC9hPiAvIDxhIGhyZWY9ImNhdGFsb2cuYXNweD9nZW5yZT0xNyI+Rm9yIHN0JiMyNDg7cnJlIGImIzI0ODtybjwvYT5kAggPZBYCZg8VCgo5MDAwMDAwMTU5CjkwMDAwMDAxNTkIQWxsIEJveXMIQWxsIEJveXOBATxpbWcgc3JjPSIvZmplcm5sZWplL2NvbnRlbnQvaW1nL21lZGllcmFhZGV0LzExLnBuZyIgYWx0PSJUaWxsYWR0IGZvciBiw7hybiBvdmVyIDExIMOlciIgdGl0bGU9IlRpbGxhZHQgZm9yIGLDuHJuIG92ZXIgMTEgw6VyIiAvPgo5MDAwMDAwMTU5CEFsbCBCb3lzsAFFbiBrYXJha3RlcmRyZXZlbiBkb2t1bWVudGFyZmlsbSBvbSBiw7hzc2Vwb3Juby1pbmR1c3RyaWVuIGkgRXVyb3BhLiBNZWQgdWRnYW5nc3B1bmt0IGkgVGpla2tpZXQsIGh2b3IgYXJiZWpkc2zDuHNoZWQgaGFyIGRyZXZldCB1bmdlIG3Dpm5kIGluZCBpIGJyYW5jaGVuLCBvZyBpIEJlcmxpbiwgaHZvci4uLgY1Mm1pbiAAZAIJD2QWAmYPFQoKMjc4OTYyMjcwMAoyNzg5NjIyNzAwJEFsbCBteSBmcmllbmRzIGFyZSBsZWF2aW5nIEJyaXNiYW5lICRBbGwgbXkgZnJpZW5kcyBhcmUgbGVhdmluZyBCcmlzYmFuZSCBATxpbWcgc3JjPSIvZmplcm5sZWplL2NvbnRlbnQvaW1nL21lZGllcmFhZGV0LzE1LnBuZyIgYWx0PSJUaWxsYWR0IGZvciBiw7hybiBvdmVyIDE1IMOlciIgdGl0bGU9IlRpbGxhZHQgZm9yIGLDuHJuIG92ZXIgMTUgw6VyIiAvPgoyNzg5NjIyNzAwJEFsbCBteSBmcmllbmRzIGFyZSBsZWF2aW5nIEJyaXNiYW5lIMUBQW50aGVhIGVyIGkgMjAnZXJuZSwgc2luZ2xlLCBoYXIgZXQga2VkZWxpZ3Qgam9iIG9nIGJvciBkZXN1ZGVuIGkgcGFyZm9yaG9sZHNieWVuIEJyaXNiYW5lLCBBdXN0cmFsaWVuLiBBbGxlIGhlbmRlcyB2ZW5uZXIgZmx5dHRlciBmcmEgYnllbiBlbGxlciBibGl2ZXIgZ2lmdC4gSHVuIG92ZXJ2ZWplciwgb20gaHVuIHNrYWwgZsO4bGdlIG1lZC4JMXQgMTZtaW4gKjxhIGhyZWY9ImNhdGFsb2cuYXNweD9nZW5yZT00Ij5Lb21lZGllPC9hPmQCCg9kFgJmDxUKCjI0MjgyMDkwMDAKMjQyODIwOTAwMAxBbGxhaHMgYsO4cm4MQWxsYWhzIGLDuHJuZjxpbWcgc3JjPSIvZmplcm5sZWplL2NvbnRlbnQvaW1nL21lZGllcmFhZGV0L2EucG5nIiBhbHQ9IlRpbGxhZHQgZm9yIGFsbGUiIHRpdGxlPSJUaWxsYWR0IGZvciBhbGxlIiAvPgoyNDI4MjA5MDAwDEFsbGFocyBiw7hybq8BTWVucyBkZW4gaGVrdGlza2UgcGFraXN0YW5za2Ug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IGh2ZXJkYWdzaGFybW9uaSBtZWQgZGVyZXMgbGlsbGUgc8O4biBPc2NhciwgbWVuIGVuIGRhZyB0YWdlciB0aWx2w6ZyZWxzZW4gZW4gZHJhbWF0aXNrIHZlbmRpbmcuIEJldsOmYm5ldCBwb2xpdGkgYmVzdG9ybWVyIHBsdWRzZWxpZy4uLgkxdCAzNm1pbiBWPGEgaHJlZj0iY2F0YWxvZy5hc3B4P2dlbnJlPTIiPkRyYW1hPC9hPiAvIDxhIGhyZWY9ImNhdGFsb2cuYXNweD9nZW5yZT04Ij5UaHJpbGxlcjwvYT5kAg8PZBYCZg8VCgoyNjY3MTE3NTAwCjI2NjcxMTc1MDARQW1lcmljYW4gc3BsZW5kb3IRQW1lcmljYW4gc3BsZW5kb3KBATxpbWcgc3JjPSIvZmplcm5sZWplL2NvbnRlbnQvaW1nL21lZGllcmFhZGV0LzE1LnBuZyIgYWx0PSJUaWxsYWR0IGZvciBiw7hybiBvdmVyIDE1IMOlciIgdGl0bGU9IlRpbGxhZHQgZm9yIGLDuHJuIG92ZXIgMTUgw6VyIiAvPgoyNjY3MTE3NTAwEUFtZXJpY2FuIHNwbGVuZG9yVEhhcnZleSBQZWthciBvcHRyw6ZkZXIgbGl2ZSBzb20gc2lnIHNlbHYgc29tIHRlZ25lc2VyaWVza2FiZXJlbiwgZGVyIGlra2Uga2FuIHRlZ25lLgkxdCAzN21pbiAAZAIQD2QWAmYPFQoKMjc3NzY3NjAwMAoyNzc3Njc2MDAwBUFuZ2VsBUFuZ2VsgQE8aW1nIHNyYz0iL2ZqZXJubGVqZS9jb250ZW50L2ltZy9tZWRpZXJhYWRldC8xMS5wbmciIGFsdD0iVGlsbGFkdCBmb3IgYsO4cm4gb3ZlciAxMSDDpXIiIHRpdGxlPSJUaWxsYWR0IGZvciBiw7hybiBvdmVyIDExIMOlciIgLz4KMjc3NzY3NjAwMAVBbmdlbLMBVW5nZSwgc211a2tlIEFuZ2VsIERldmVyZWxsIGRyw7htbWVyIG9tIGF0IGJsaXZlIGVuIHN0b3IgZm9yZmF0dGVyLCBodW4gbGV2ZXIgaSBmYW50YXNpZW5zIHZlcmRlbiwgc25hcmVyZSBlbmQgaSBkZW4gZ3LDpSB2aXJrZWxpZ2hlZCBob3Mgc2luIG1vciwgZGVyIGRyaXZlciBlbiBrw7hibWFuZHNoYW5kZWwuLi4JMXQgNTRtaW4gXDxhIGhyZWY9ImNhdGFsb2cuYXNweD9nZW5yZT0yIj5EcmFtYTwvYT4gLyA8YSBocmVmPSJjYXRhbG9nLmFzcHg/Z2VucmU9NSI+SyYjMjMwO3JsaWdoZWQ8L2E+ZAIRD2QWAmYPFQoKOTAwMDAwMDE3Mwo5MDAwMDAwMTczBUFuZ2VsBUFuZ2VsqAE8aW1nIHNyYz0iL2ZqZXJubGVqZS9jb250ZW50L2ltZy9tZWRpZXJhYWRldC83LnBuZyIgYWx0PSJUaWxsYWR0IGZvciBhbGxlLCBtZW4gZnJhcsOlZGVzIGLDuHJuIHVuZGVyIDcgw6VyIiB0aXRsZT0iVGlsbGFkdCBmb3IgYWxsZSwgbWVuIGZyYXLDpWRlcyBiw7hybiB1bmRlciA3IMOlciIgLz4KOTAwMDAwMDE3MwVBbmdlbL8BRXQgcm9ja2JhbmQgaGFyIGZvcmzDpm5nc3QgcGFzc2VyZXQga2FycmllcmVucyBow7hqZGVwdW5rdCwgbWVuIG3DpXNrZSBrYW4gbWFuIG9wbsOlIG55IGJlcsO4bW1lbHNlIHZlZCBhdCBkw7g/IERldCBsb2trZXIgUmljaGFyZCBzaW4ga29uZSwgc2FuZ2VyaW5kZW4gQW5nZWwgdGlsLCBtZW4gZm9yc3ZpbmRpbmdzbnVtbWVyZXQuLi4JMXQgNDRtaW4gKDxhIGhyZWY9ImNhdGFsb2cuYXNweD9nZW5yZT0yIj5EcmFtYTwvYT5kAhIPZBYCZg8VCgo5MDAwMDAwMjMyCjkwMDAwMDAyMzIRQW5rbGFnZXQgZm9yIG1vcmQRQW5rbGFnZXQgZm9yIG1vcmSoATxpbWcgc3JjPSIvZmplcm5sZWplL2NvbnRlbnQvaW1nL21lZGllcmFhZGV0LzcucG5nIiBhbHQ9IlRpbGxhZHQgZm9yIGFsbGUsIG1lbiBmcmFyw6VkZXMgYsO4cm4gdW5kZXIgNyDDpXIiIHRpdGxlPSJUaWxsYWR0IGZvciBhbGxlLCBtZW4gZnJhcsOlZGVzIGLDuHJuIHVuZGVyIDcgw6VyIiAvPgo5MDAwMDAwMjMyEUFua2xhZ2V0IGZvciBtb3JksgFUb20gQ2hvbG1vbmRlbGV5IGhhciB0aWRsaWdlcmUgc3TDpWV0IGFua2xhZ2V0IGZvciBhdCBoYXZlIHNrdWR0IGVuIHdpbGQgbGlmZSBzZXJ2aWNlLW1lZGFyYmVqZGVyLCBkZXIgb3Bob2xkdCBzaWcgcMOlIGhhbnMgZW5vcm1lIGxhbmRlamVuZG9tIGkgS2VueWEuIENob2xtb25kZWxleSwgZW5lYXJ2aW5nLi4uBjUzbWluIABkAhMPZBYCZg8VCgoyNDU2MDUxMTAwCjI0NTYwNTExMDAJQW5uYXMgZGFnCUFubmFzIGRhZ2Y8aW1nIHNyYz0iL2ZqZXJubGVqZS9jb250ZW50L2ltZy9tZWRpZXJhYWRldC9hLnBuZyIgYWx0PSJUaWxsYWR0IGZvciBhbGxlIiB0aXRsZT0iVGlsbGFkdCBmb3IgYWxsZSIgLz4KMjQ1NjA1MTEwMAlBbm5hcyBkYWezAURlciBrb21tZXIgZW4gZGFnIGkgZXRodmVydCBtZW5uZXNrZXMgbGl2LCBodm9yIGhlbGUgdmVyZGVuIHN5bmVzIGF0IHJhbWxlIHNhbW1lbiBvdmVyIMOpbi4gQW5uYSBlciBlbiAzMC3DpXJpZyBlbmxpZyBtb3IsIHNvbSBib3IgaSBLw7hiZW5oYXZuIG1lZCBzaW4gc8O4biBFbWlsLiBPZyBpIGRhZyBza2FsLi4uBjMwbWluIABkAhQPZBYCZg8VCgoyNTYyOTAxOTAwCjI1NjI5MDE5MDAIQW5zaWd0ZXQIQW5zaWd0ZXSBATxpbWcgc3JjPSIvZmplcm5sZWplL2NvbnRlbnQvaW1nL21lZGllcmFhZGV0LzE1LnBuZyIgYWx0PSJUaWxsYWR0IGZvciBiw7hybiBvdmVyIDE1IMOlciIgdGl0bGU9IlRpbGxhZHQgZm9yIGLDuHJuIG92ZXIgMTUgw6VyIiAvPgoyNTYyOTAxOTAwCEFuc2lndGV0mQFEci4gVm9nbGVycyBNYWduZXRpc2tlIEhlbHNldGVhdGVyIG9wdHLDpmRlciBpIDE4NDYgaSBrb25zdWwgRWdlcm1hbnMgaGplbS4gRGVyZXMgbnVtcmUgYWZzbMO4cmVzIHNvbSBmdXAsIG1lbiBhbGxpZ2V2ZWwgc2VqcmVyIGt1bnN0ZW4gb3ZlciB2aWRlbnNrYWJlbi4JMXQgMzdtaW4gKDxhIGhyZWY9ImNhdGFsb2cuYXNweD9nZW5yZT0yIj5EcmFtYTwvYT5kAhUPZBYCZg8VCgoyNTM0MDMyOTAwCjI1MzQwMzI5MDAOQW55dGhpbmcgZWxzZSAOQW55dGhpbmcgZWxzZSBmPGltZyBzcmM9Ii9mamVybmxlamUvY29udGVudC9pbWcvbWVkaWVyYWFkZXQvYS5wbmciIGFsdD0iVGlsbGFkdCBmb3IgYWxsZSIgdGl0bGU9IlRpbGxhZHQgZm9yIGFsbGUiIC8+CjI1MzQwMzI5MDAOQW55dGhpbmcgZWxzZSCiAUplcnJ5IGRyw7htbWVyIG9tIGF0IHNrcml2ZSBlbiByb21hbiwgbWVuIGhhbnMgdGlkIGfDpXIgbWVkIGF0IGhvbGRlIHN0eXIgcMOlIHNpbiBtYW5pcHVsZXJlbmRlIGvDpnJlc3RlLCBkZXIgZXIgZ29kdCBww6UgdmVqIHRpbCBhdCBnw7hyZSBoYW5zIGxpdiB0aWwgZXQgaGVsdmVkZQkxdCA0NG1pbiBePGEgaHJlZj0iY2F0YWxvZy5hc3B4P2dlbnJlPTQiPktvbWVkaWU8L2E+IC8gPGEgaHJlZj0iY2F0YWxvZy5hc3B4P2dlbnJlPTUiPksmIzIzMDtybGlnaGVkPC9hPmQCFg9kFgJmDxUKCjI3MDY0NTgyMDAKMjcwNjQ1ODIwMApBcG9jYWx5cHRvCkFwb2NhbHlwdG+BATxpbWcgc3JjPSIvZmplcm5sZWplL2NvbnRlbnQvaW1nL21lZGllcmFhZGV0LzE1LnBuZyIgYWx0PSJUaWxsYWR0IGZvciBiw7hybiBvdmVyIDE1IMOlciIgdGl0bGU9IlRpbGxhZHQgZm9yIGLDuHJuIG92ZXIgMTUgw6VyIiAvPgoyNzA2NDU4MjAwCkFwb2NhbHlwdG+yAU1heWFlcm5lcyBtYWd0IGVyIHDDpSBzaXQgaMO4amVzdGUsIG1lbiBNYXlhIGt1bHR1cmVucyBla3Npc3RlbnMgZXIgc3TDpnJrdCB0cnVldCBpbmRlZnJhIGFmIGV0IHN0YWRpZ3QgbWVyZSBkZWthZGVudCBvZyBrcsOmdmVuZGUgc3R5cmUsIG9nIHVkZWZyYSBhZiBkZSBzcGFuc2tlIGVyb2JyZXJlLCBkZXIuLi4JMnQgMTJtaW4g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FgICAQ8PFgQfCgUCNDUfAwUCNDVkZAJaD2QWAgIBDw8WBB8KBQI0Nh8DBQI0NmRkAlwPZBYCAgEPDxYEHwoFAjQ3HwMFAjQ3ZGQCDQ8WAh8HAg0WGgIBD2QWAmYPFQIkL2ZqZXJubGVqZS9maWxtL2NhdGFsb2cuYXNweD9nZW5yZT0xBkFjdGlvbmQCAg9kFgJmDxUCJC9mamVybmxlamUvZmlsbS9jYXRhbG9nLmFzcHg/Z2VucmU9MgVEcmFtYWQCAw9kFgJmDxUCJC9mamVybmxlamUvZmlsbS9jYXRhbG9nLmFzcHg/Z2VucmU9MwVHeXNlcmQCBA9kFgJmDxUCJC9mamVybmxlamUvZmlsbS9jYXRhbG9nLmFzcHg/Z2VucmU9NAdLb21lZGllZAIFD2QWAmYPFQIlL2ZqZXJubGVqZS9maWxtL2NhdGFsb2cuYXNweD9nZW5yZT0xNQVLcmltaWQCBg9kFgJmDxUCJC9mamVybmxlamUvZmlsbS9jYXRhbG9nLmFzcHg/Z2VucmU9NQpLw6ZybGlnaGVkZAIHD2QWAmYPFQIkL2ZqZXJubGVqZS9maWxtL2NhdGFsb2cuYXNweD9nZW5yZT02BU11c2lrZAIID2QWAmYPFQIkL2ZqZXJubGVqZS9maWxtL2NhdGFsb2cuYXNweD9nZW5yZT03D1NjaWVuY2UgRmljdGlvbmQCCQ9kFgJmDxUCJC9mamVybmxlamUvZmlsbS9jYXRhbG9nLmFzcHg/Z2VucmU9OAhUaHJpbGxlcmQCCg9kFgJmDxUCJC9mamVybmxlamUvZmlsbS9jYXRhbG9nLmFzcHg/Z2VucmU9OQxVbmRlcnZpc25pbmdkAgsPZBYCZg8VAiUvZmplcm5sZWplL2ZpbG0vY2F0YWxvZy5hc3B4P2dlbnJlPTE2DkZvciBzbcOlIGLDuHJuZAIMD2QWAmYPFQIlL2ZqZXJubGVqZS9maWxtL2NhdGFsb2cuYXNweD9nZW5yZT0xNxFGb3Igc3TDuHJyZSBiw7hybmQCDQ9kFgJmDxUCJS9mamVybmxlamUvZmlsbS9jYXRhbG9nLmFzcHg/Z2VucmU9MTEIRm9yIHVuZ2VkGAEFHl9fQ29udHJvbHNSZXF1aXJlUG9zdEJhY2tLZXlfXxYCBRxjdGwwMCRNYWluQ29udGVudCRQcmV2QnV0dG9uBRxjdGwwMCRNYWluQ29udGVudCROZXh0QnV0dG9ui3s/5MT6vWnacTWH2g7OdJbjtgk="}
                                   };
                var doc = PostDocument(postData, _crawlstart);
                page++;


                HtmlNodeCollection list = doc.DocumentNode.SelectNodes("//a[@class='title']");

                foreach (HtmlNode htmlNode in list)
                {
                    moviesToLoad.Add("http://www.voddler.com" + htmlNode.Attributes["href"].Value);                        
                }

                if (doc.DocumentNode.SelectSingleNode("//a[@class='goright inactive']") != null)
                {
                    resultContainsMovies = false;
                }
            }

            StartedThreads = moviesToLoad.Count;

            foreach (var movie in moviesToLoad)
            {
                ThreadPool.QueueUserWorkItem(LoadMovie, movie);
            }

            DoneEvent.WaitOne();
        }

        public void LoadMovie(object obj)
        {
            try
            {
                var repository = new FilmsterRepository();
                var movieUrl = (string)obj;

                var doc = GetDocument(movieUrl).DocumentNode;

                bool highDef = false;
                const int vendorId = 3;
                int releaseYear = 0;
                float price = 0;

                var title = doc.SelectSingleNode("//div[@id='screencycle_movieinfo']//h2").InnerText.Trim();
                var plot = doc.SelectSingleNode("//p[@id='plot']").InnerText.Trim();
                var coverUrl = doc.SelectSingleNode("//div[@id='moviecovercontainer']//img").Attributes["src"].Value;

                if (title.Contains(" (HD)"))
                {
                    highDef = true;
                    title = title.Replace(" (HD)", "");
                }

                int.TryParse(doc.SelectSingleNode("//div[(@class='movie_metadata_container')]//table/tr[position()=2]/td[position()=2]").InnerText.RemoveNonNumericChars(), out releaseYear);
                float.TryParse(
                    doc.SelectSingleNode("//div[@id='buttoncontainer']//span[@class='price']").InnerText.RemoveNonNumericChars(),
                    out price);

                ResolveRentalOption(repository, movieUrl, coverUrl, vendorId, title, plot, releaseYear, false, highDef, price);
                repository.Save();

                Logger.LogVerbose(title.Trim());


            }
            catch (Exception ex)
            {
                LogCrawlerError(ex);
            }

            if (Interlocked.Decrement(ref StartedThreads) == 0)
            {
                DoneEvent.Set();
            }

        }
    }
}