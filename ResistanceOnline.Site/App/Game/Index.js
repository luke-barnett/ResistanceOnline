define(['data', 'knockout', 'knockout.punches'], function (data, ko, kop) {
<<<<<<< HEAD
    ko.punches.enableAll();

    var viewModel = { index: ko.observable(0), game: ko.computed(function () { return data()[viewModel.index()]; }, this, { deferEvaluation: true }) };
    viewModel.activate = function (index) { viewModel.index(index); };
=======
    var viewModel = { game: ko.observable() };

    viewModel.activate = function (index) { viewModel.game(data[index]); };
>>>>>>> knockouts

    return viewModel;
});