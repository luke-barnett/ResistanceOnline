define(['data', 'knockout', 'knockout.punches'], function (data, ko, kop) {
    var viewModel = {
        gameId: ko.observable(),
        start: function () {
            $.connection.gameHub.server.startGame(viewModel.gameId());
        },
        activate: function (game) {
            viewModel.gameId(game.GameId());
        }
    };

    return viewModel;
});