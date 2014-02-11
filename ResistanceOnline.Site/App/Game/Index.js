define(['data', 'knockout', 'knockout.punches'], function (data, ko, kop) {
    ko.punches.enableAll();

    var viewModel = { game: ko.observable() };

    var deferred = new $.Deferred();

    //todo index
    viewModel.game(data()[2]);

    if (data().length)
        deferred.resolve();

    //todo index
    data.subscribe(function () { viewModel.game(data()[2]); deferred.resolve(); });

    viewModel.activate = function (index) { return deferred; };

    return viewModel;
});