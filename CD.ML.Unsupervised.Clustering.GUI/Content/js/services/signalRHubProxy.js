'use strict';

angular.module('clusteringApp').factory('signalRHubProxy', ['$rootScope',
    function ($rootScope) {

        function signalRHubProxyFactory(hubName, startOptions) {

            var connection = $.hubConnection();
            var proxy = connection.createHubProxy(hubName);
            connection.start(startOptions).done(function () { });

            return {
                on: function (eventName, callback) {
                    proxy.on(eventName, function (result) {
                        $rootScope.$apply(function () {
                            if (callback) {
                                callback(result);
                            }
                        });
                    });
                },
                off: function (eventName, callback) {
                    proxy.off(eventName, function (result) {
                        $rootScope.$apply(function () {
                            if (callback) {
                                callback(result);
                            }
                        });
                    });
                },
                invoke: function (methodName, args) {
                    return proxy.invoke.apply(proxy, arguments);
                },
                connection: connection
            };
        };

        return signalRHubProxyFactory;
    }]);