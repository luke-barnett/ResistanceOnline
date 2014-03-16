define(['data', 'knockout', 'knockout.punches'], function (data, ko, kop) {
    var viewModel = {
        gameId: ko.observable(),
        players: ko.observableArray(),
        addToTeam: function (player) {
            $.connection.gameHub.server.addToTeam(viewModel.gameId(), player);
        },
        activate: function(game) {            
            viewModel.gameId(game.GameId());
            viewModel.players(game.AddToTeamPlayersSelectList());
        }
    };

    return viewModel;
});