define(['data', 'knockout', 'knockout.punches'], function (data, ko, kop) {
<<<<<<< HEAD
<<<<<<< HEAD
    ko.punches.enableAll();

    var viewModel = { index: ko.observable(0), game: ko.computed(function () { return data()[viewModel.index()]; }, this, { deferEvaluation: true }) };
    viewModel.activate = function (index) { viewModel.index(index); };
=======
    var viewModel = { game: ko.observable() };

    viewModel.activate = function (index) { viewModel.game(data[index]); };
>>>>>>> knockouts
=======
    ko.punches.enableAll();

    var viewModel = { game: ko.observable() };

    var deferred = new $.Deferred();

    //todo index
    data.subscribe(function () { viewModel.game(data()[0]); deferred.resolve(); });

    viewModel.activate = function (index) { return deferred; };
>>>>>>> more knockout

    return viewModel;
});