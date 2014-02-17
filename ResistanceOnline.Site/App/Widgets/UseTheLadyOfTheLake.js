define(['data', 'knockout', 'knockout.punches'], function (data, ko, kop) {
    var viewModel = {
        gameId: ko.observable(),
        player: ko.observable(),
        players: ko.observableArray(),
        useTheLadyOfTheLake: function () {
            $.connection.gameHub.server.ladyOfTheLake(viewModel.gameId(), viewModel.player().Text());
        },
        activate: function (game) {
            viewModel.gameId(game.GameId());
            viewModel.players(game.LadyOfTheLakePlayerSelectList());
        }
    };

    return viewModel;
});