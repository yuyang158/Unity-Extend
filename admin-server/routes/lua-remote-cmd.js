const express = require('express');
const Router = express.Router();
const request = require('request');

Router.get('/lrc/devices', (req, res) => {
    request.get('http://private-tunnel.site:4100/?cmd=devices').on('response', (response) => {
        response.on('data', data => {
            const ret = data.toString('utf8');
            res.json({
                code: 20000,
                content: ret
            });
        })
    });
});

Router.post('/lrc/cmd', (req, res) => {
    const device = req.body.device;
    request.post(`http://private-tunnel.site:4100/?cmd=lua&device=${device}`,
        {
            body: req.body.lua,
            'content-type': 'text/plain'
        }).on('response', response => {
        response.on('data', data => {
            const ret = data.toString('utf8');
            res.json({
                code: 20000,
                content: ret
            });
        })
    }).on('error', err => {
        console.log(err)
    });
});

module.exports = Router;