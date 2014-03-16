define(['data', 'knockout', 'knockout.punches'], function (data, ko, kop) {
    var viewModel = {
        gameId: ko.observable(),
        rules: ko.observableArray(),
        addRule: function (rule) {
            $.connection.gameHub.server.addRule(viewModel.gameId(), rule);
        },
        activate: function(game) {            
            viewModel.gameId(game.GameId());
            viewModel.rules(game.AddRulesSelectList());
        }
    };

    return viewModel;
});