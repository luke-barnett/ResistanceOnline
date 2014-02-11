define(['data', 'knockout', 'knockout.punches'], function (data, ko, kop) {
    ko.punches.enableAll();

    var viewModel = { game: ko.observable() };

    var deferred = new $.Deferred();

    //todo index
    data.subscribe(function () { viewModel.game(data()[0]); deferred.resolve(); });

    viewModel.activate = function (index) { return deferred; };

    return viewModel;
});