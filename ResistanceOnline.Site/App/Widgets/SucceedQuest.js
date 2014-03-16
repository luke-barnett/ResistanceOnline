define(['data', 'knockout', 'knockout.punches'], function (data, ko, kop) {
    var viewModel = {
        gameId: ko.observable(),
        approve: ko.observable(),
        succeed: function () {
            $.connection.gameHub.server.succeedQuest(viewModel.gameId());
        },
        activate: function (game) {
            viewModel.gameId(game.GameId());
        }
    };

    return viewModel;
});