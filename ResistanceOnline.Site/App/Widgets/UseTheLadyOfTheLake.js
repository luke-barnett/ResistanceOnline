define(['data', 'knockout', 'knockout.punches'], function (data, ko, kop) {
    var viewModel = {
        gameId: ko.observable(),
        players: ko.observableArray(),
        useTheLadyOfTheLake: function (player) {
            $.connection.gameHub.server.ladyOfTheLake(viewModel.gameId(), player);
        },
        activate: function (game) {
            viewModel.gameId(game.GameId());
            viewModel.players(game.LadyOfTheLakePlayerSelectList());
        }
    };

    return viewModel;
});