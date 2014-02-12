<<<<<<< HEAD
﻿define(['data', 'knockout', 'knockout.punches'], function (data, ko, kop) {        
    var viewModel = {
        gameId: ko.observable(),
        characters: ko.observableArray([]),
        character: ko.observable(''),
        addCharacter: function () {
            $.connection.gameHub.server.addCharacter(viewModel.gameId(), viewModel.character());            
        },
        activate: function(game) {            
            viewModel.gameId(game.GameId());
            viewModel.characters(game.AvailableCharacters());
=======
﻿define(['data', 'knockout', 'knockout.punches'], function (data, ko, kop) {
    console.log('not u');
    //todo ctor?
    var viewModel = {
        gameId: ko.observable(),
        name: ko.observable(''),
        joinGame: function () {
            $.connection.gameHub.server.joinGame(viewModel.gameId(), viewModel.name());
            //player guid?
        },
        activate: function(game) {            
            viewModel.gameId(game.GameId());
>>>>>>> convert actions to widgets
        }
    };

    return viewModel;
});