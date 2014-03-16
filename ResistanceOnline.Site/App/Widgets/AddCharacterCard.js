define(['data', 'knockout', 'knockout.punches'], function (data, ko, kop) {
    var viewModel = {
        gameId: ko.observable(),
        cards: ko.observableArray(),
        addCharacterCard: function (card) {
            $.connection.gameHub.server.addCharacterCard(viewModel.gameId(), card);
        },
        activate: function(game) {            
            viewModel.gameId(game.GameId());
            viewModel.cards(game.AddCharacterCardsSelectList());
        }
    };

    return viewModel;
});