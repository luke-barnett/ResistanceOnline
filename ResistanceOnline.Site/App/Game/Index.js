define(['data', 'knockout', 'knockout.punches', 'bootstrap'], function (data, ko, kop) {
    ko.punches.enableAll();
    var deferred = new $.Deferred();
    data.subscribe(function () { deferred.resolve() });
    var viewModel = { index: ko.observable(0), game: ko.computed(function () { return data()[viewModel.index()]; }, this, { deferEvaluation: true }) };
    viewModel.activate = function (index) { viewModel.index(index); if(!data().length) return deferred;};

    return viewModel;
});