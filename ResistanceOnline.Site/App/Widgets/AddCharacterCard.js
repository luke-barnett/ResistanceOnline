define(['data', 'knockout', 'knockout.punches'], function (data, ko, kop) {
    var viewModel = {
        gameId: ko.observable(),
        card: ko.observable(),
        cards: ko.observableArray(),
        addToTeam: function () {
            $.connection.gameHub.server.addCharacterCard(viewModel.gameId(), viewModel.card().Text());
        },
        activate: function(game) {            
            viewModel.gameId(game.GameId());
            viewModel.cards(game.AddCharacterCardsSelectList());
        }
    };

    return viewModel;
});