define(['data', 'knockout', 'knockout.punches'], function (data, ko, kop) {
    var viewModel = {
        gameId: ko.observable(),
        joinGame: function () {
            $.connection.gameHub.server.joinGame(viewModel.gameId());
        },
        activate: function (game) {
            console.log(game.GameId());
            viewModel.gameId(game.GameId());
        }
    };

    return viewModel;
});