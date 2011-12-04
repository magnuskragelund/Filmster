/// <reference path="~/Content/scripts/jquery-1.5.1.js">
$(function () {
    $('.movie-scroller').mbScrollable({
        width: 800,
        elementsInPage: 1,
        autoscroll: true
    });

    $("#queryTextbox").autocomplete("/Filmster/autocomplete", {
        delay: 0,
        scroll: false,
        parse: function (items) {
            var parsed = [];
            for (var i = 0; i < items.length; i++) {
                var item = items[i];
                if (item) {
                    parsed[parsed.length] = {
                        data: item,
                        value: item.Title,
                        result: item.Title
                    };
                }
            }
            return parsed;
        },
        formatItem: function (item) {
            return item.Title;
        },
        selectFirst: false
    }).result(function (event, item) {
        location.href = item.Url;
    });

    $('.rent-column a').click(function (e) {
        _gaq.push(['_trackEvent', 'Film', 'Lej', $(e.currentTarget).attr('data-moviename')]);
    });
});
