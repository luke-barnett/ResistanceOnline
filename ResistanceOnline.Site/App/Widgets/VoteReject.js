define(['data', 'knockout', 'knockout.punches'], function (data, ko, kop) {
    var viewModel = {
        gameId: ko.observable(),
        approve: ko.observable(),
        reject: function () {
            $.connection.gameHub.server.voteReject(viewModel.gameId());
        },
        activate: function(game) {            
            viewModel.gameId(game.GameId());
        }
    };

    return viewModel;
});