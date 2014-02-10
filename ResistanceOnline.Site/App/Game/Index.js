define(['data', 'knockout', 'knockout.punches'], function (data, ko, kop) {
    var viewModel = { game: ko.observable() };

    viewModel.activate = function (index) { viewModel.game(data[index]); };

    return viewModel;
});