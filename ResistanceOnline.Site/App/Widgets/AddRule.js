define(['data', 'knockout', 'knockout.punches'], function (data, ko, kop) {
    var viewModel = {
        gameId: ko.observable(),
        rule: ko.observable(),
        rules: ko.observableArray(),
        addToTeam: function () {
            $.connection.gameHub.server.addRule(viewModel.gameId(), viewModel.rule().Text());
        },
        activate: function(game) {            
            viewModel.gameId(game.GameId());
            viewModel.rules(game.AddRulesSelectList());
        }
    };

    return viewModel;
});