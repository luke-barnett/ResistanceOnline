define(['signalr.hubs', 'knockout', 'knockout.mapping'], function (signalr, ko, map) {
    var games = ko.observableArray();

    function PlayerInfo(data, game) {
        var self = this;
        map.fromJS(data, {}, this);

        self.showActual = ko.observable(false);
        self.image = ko.computed(function () {
            if (self.showActual() || game.GameOver()) {
                return "/Images/" + data.Actual + ".png";
            } else {
                return "/Images/player.png";
            }
        });

        self.toggleShowActual = function () {
            self.showActual(!self.showActual());
        }

        return this;
    }


    function Character(data, game) {
        var self = this;
        map.fromJS(data, {}, this);
        
        self.image = ko.computed(function () {
           return "/Images/" + data + ".png";
        });

        self.name = data;

        self.setCharacter = function (index) {
            $.connection.gameHub.server.setCharacter(game.GameId(), index, this.Value());
        }

        return this;
    }

   
    var update = function (g) {
        var mapping = {
            PlayerInfo: { create: function (options) { return new PlayerInfo(options.data, options.parent); } },
            CharactersInGame: { create: function (options) { return new Character(options.data, options.parent); } }
        };
        map.fromJS(g, mapping, games);
    };

    var gameHub = $.connection.gameHub;
    gameHub.on('update', update);
    $.connection.hub.start();

    return games;
});