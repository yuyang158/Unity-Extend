const express = require('express');
const app = express();
const mysql = require('mysql');
const async = require('async');

const connection = mysql.createPool({
    connectionLimit : 10,
    host: '127.0.0.1',
    user: 'root',
    password: '123456',
    database: 'translate'
})

// parse application/x-www-form-urlencoded
app.use(express.urlencoded({ extended: false }))

// parse application/json
app.use(express.json({limit: '1000mb'}))
app.use(express.static('public'))
app.post('/', (req, res) => {
    const sheetName = req.body.sheetName;
    try {
        async.waterfall([
            cb => {
                connection.query('CREATE TABLE IF NOT EXISTS ?? (\
                    `id` varchar(64) NOT NULL,\
                    `cn` text,\
                    PRIMARY KEY (`id`)\
                  ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;', [sheetName], err => cb(err))
            },
            cb => {
                async.each(req.body.data, (row, cb) => {
                    connection.query('SELECT `cn` FROM ?? WHERE `id` = ?;', [sheetName, row[0]], (err, results) => {
                        if(err) {
                            cb(err)
                        }
                        else {
                            if(results.length == 0) {
                                connection.query('INSERT INTO ?? (`id`, `cn`) VALUES (?, ?)', [sheetName, row[0], row[1]], cb)
                            }
                            else {
                                const val = results[0]
                                if(val.cn == row[1]) {
                                    cb(null)
                                }
                                else {
                                    connection.query('UPDATE ?? SET `cn` = ? WHERE `id` = ?', [sheetName, row[1], row[0]], cb);
                                }
                            }
                        }
                    })                  
                }, err => cb(err))
            },
            cb => {
                connection.query(`SELECT * FROM ${sheetName};`, (err, response) => {
                    if(err) {
                        cb(err);
                    }
                    else {
                        let ret = '';
                        const keys = Object.keys(response[0]);
                        ret += keys.join('\t');
                        ret += '\r\n';
                        
                        const types = [];
                        for (let index = 0; index < keys.length; index++) {
                            types.push('string');
                        }
                        ret += types.join('\t');
                        ret += '\r\n';
                        for (let index = 0; index < response.length; index++) {
                            const element = response[index];
                            const values = Object.values(element);
                            ret += values.join('\t');
                            ret += '\r\n'
                        }
                        cb(null, ret);
                    }
                })
            }
        ], (err, ret) => {
            if(err) {
                res.json({code: 1, err: err})
            }
            else {
                res.json({code: 0, tsv: ret})
            }
        })
    } 
    catch (error) {
        
    }
});

app.listen(30000, () => {console.log('server start listen 30000')});