define(['data', 'knockout', 'knockout.punches'], function (data, ko, kop) {
    ko.punches.enableAll();

    var viewModel = { index: ko.observable(0), game: ko.computed(function () { return data()[viewModel.index()]; }, this, { deferEvaluation: true }) };
    viewModel.activate = function (index) { viewModel.index(index); };

    return viewModel;
});