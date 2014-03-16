define(['data', 'knockout', 'knockout.punches'], function (data, ko, kop) {
    var viewModel = {
        gameId: ko.observable(),
        approve: ko.observable(),
        accept: function () {
            $.connection.gameHub.server.voteApprove(viewModel.gameId());
        },
        activate: function(game) {            
            viewModel.gameId(game.GameId());
        }
    };

    return viewModel;
});