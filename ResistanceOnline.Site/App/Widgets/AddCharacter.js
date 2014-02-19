define(['data', 'knockout', 'knockout.punches'], function (data, ko, kop) {        
    var viewModel = {
        gameId: ko.observable(),
        characters: ko.observableArray([]),
        character: ko.observable(),
        addCharacter: function () {            
            $.connection.gameHub.server.addCharacter(viewModel.gameId(), viewModel.character().Value());            
        },
        activate: function(game) {            
            viewModel.gameId(game.GameId());
            viewModel.characters(game.AllCharactersSelectList());
        }
    };

    return viewModel;
});