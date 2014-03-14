define(['data', 'knockout', 'knockout.punches'], function (data, ko, kop) {
    var viewModel = {
        gameId: ko.observable(),
        addBot: function () {
            $.connection.gameHub.server.addBot(viewModel.gameId());
        },
        activate: function (game) {
            viewModel.gameId(game.GameId());
        }
    };

    return viewModel;
});