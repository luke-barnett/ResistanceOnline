define(['data', 'knockout', 'knockout.punches'], function (data, ko, kop) {
    var viewModel = {
        gameId: ko.observable(),
        approve: ko.observable(),
        voteForTeam: function () {
            $.connection.gameHub.server.voteForTeam(viewModel.gameId(), viewModel.approve());
        },
        activate: function(game) {            
            viewModel.gameId(game.GameId());
        }
    };

    return viewModel;
});