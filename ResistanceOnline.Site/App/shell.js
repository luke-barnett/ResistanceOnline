define(['require', 'plugins/router', 'signalr.hubs'], function (require, router, signalr) {
  
    return {
        router: router,
        activate: function () {
            router.map([
              { route: '', title: 'Home', moduleId: 'home/index', nav: true },
              { route: 'game/:gameId', title: 'Game', moduleId: 'game/index', nav: true },
            ]).buildNavigationModel();

            return router.activate();
        }
    };
});