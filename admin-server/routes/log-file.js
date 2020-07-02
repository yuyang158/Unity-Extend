const express = require('express');
const Router = express.Router();
const shortUUID = require('short-uuid');
const uuid = shortUUID(shortUUID.constants.flickrBase58);
const path = require('path');
const mysql = require('mysql');
const sqlConnection = mysql.createConnection({
    host: '127.0.0.1',
    port: 3306,
    user: 'root',
    password: '123456',
    database: 'upload'
});

Router.post('/upload/log', (req, res) => {
    if (!req.files || Object.keys(req.files).length === 0) {
        return res.status(400).send('No files were uploaded.');
    }

    const device = req.query.device;
    const os = req.query.os;
    const version = req.query.version;
    const other = req.query.other;

    for(const k of Object.keys(req.files)) {
        const file = req.files[k];
        const guid = uuid.generate();
        const filename = `${device}-${os}-${version}-${other}-${guid}.log`;
        file.mv(`./log/${filename}`);

        sqlConnection.query('INSERT INTO `log` (`path`) VALUES (?);', [filename], (err, result) => {
            if(err) {
                console.error(err);
            }
            else {
                console.log(`Insert success ${result.insertId}`);
            }
        });
    }

    res.send('ok').end();
});

Router.post('/upload/stat', (req, res) => {
    if (!req.files || Object.keys(req.files).length === 0) {
        return res.status(400).send('No files were uploaded.');
    }

    const device = req.query.device;
    const os = req.query.os;
    const version = req.query.version;
    const other = req.query.other;

    for(const k of Object.keys(req.files)) {
        const file = req.files[k];
        const guid = uuid.generate();
        const filename = `${device}-${os}-${version}-${other}-${guid}.stat`;
        file.mv(`./log/${filename}`);

        sqlConnection.query('INSERT INTO `stat` (`path`) VALUES (?);', [filename], (err, result) => {
            if(err) {
                console.error(err);
            }
            else {
                console.log(`Insert success ${result.insertId}`);
            }
        });
    }

    res.send('ok').end();
});

function queryMethod(pageIndex, typ, cb) {
    const offset = pageIndex * 50;
    sqlConnection.query('SELECT * FROM `' + typ + '` ORDER BY `id` LIMIT 50 OFFSET ?;', [offset], (err, results) => {
        if(err) {
            console.error(err);
            cb(err);
        }
        else {
            cb(null, results);
        }
    });
}

Router.get('/query/*', (req, res) => {
    queryMethod(parseInt(req.query.index),  req.params[0], (err, results) => {
        if(err) {
            console.error(err);
            res.json({code:50000, err})
        }
        else {
            res.json({code:20000, results});
        }
    })
});

Router.get('/count/*', (req, res) => {
    sqlConnection.query('SELECT count(id) from ??;', [req.params[0]], (err, result) => {
        if(err) {
            console.error(err);
            res.json({code:50000, err})
        }
        else {
            const count = result[0]['count(id)'];
            res.json({code:20000, count: count});
        }
    });
});

Router.get('/download/file', (req, res) => {
    const filename = req.query.filename;
    const p = path.resolve(`log/${filename}`);
    res.download(p);
});

module.exports = Router;