'use strict';

angular.module('clusteringApp').controller('clusteringCtrl', function ($scope, signalRHubProxy) {
    $scope.notifications = [];

    var size = 1000, clusters = 3, sleep = 500;
    var kmeansHubProxy = signalRHubProxy('kmeansHub');

    kmeansHubProxy.on('completeNotification', function (data) {
        alert("Complete");
    });

    kmeansHubProxy.on('failureNotification', function (data) {
        console.log(JSON.stringify(data));
    });

    kmeansHubProxy.on('stepNotification', function (data) {
        $scope.notifications.push(data);
    });

    $scope.fit = function () {
        kmeansHubProxy.invoke('sample', size).done(function (data) {
            kmeansHubProxy.invoke('fit', data, clusters, sleep)
                .fail(function (message) {
                    console.log(message);
                });
        });
    }; 
}); 