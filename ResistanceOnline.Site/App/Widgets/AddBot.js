define(['data', 'knockout', 'knockout.punches'], function (data, ko, kop) {        
    var viewModel = {
        gameId: ko.observable(),
        name: ko.observableArray(''),
        bot: ko.observable(''),
        addComputerPlayer: function () {
            $.connection.gameHub.server.addComputerPlayer(viewModel.gameId(), viewModel.bot(), viewModel.name());
        },
        activate: function(game) {            
            viewModel.gameId(game.GameId());
        }
    };

    return viewModel;
});