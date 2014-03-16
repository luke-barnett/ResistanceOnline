define(['data', 'knockout', 'knockout.punches'], function (data, ko, kop) {
    var viewModel = {
        gameId: ko.observable(),
        players: ko.observableArray(),
        removeFromTeam: function (player) {
            $.connection.gameHub.server.removeFromTeam(viewModel.gameId(), player);
        },
        activate: function(game) {            
            viewModel.gameId(game.GameId());
            viewModel.players(game.RemoveFromTeamSelectList());
        }
    };

    return viewModel;
});