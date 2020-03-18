const express = require('express')
const app = express()
const bodyParser = require('body-parser')
const mysql = require('mysql')
const fs = require('fs')
const DEFAULT_LANG = 'zh-s'

const connection = mysql.createConnection({
    'host': '127.0.0.1',
    'user': 'root',
    'password': '123456',
    'database': 'translate'
})
connection.connect()

// parse application/x-www-form-urlencoded
app.use(bodyParser.urlencoded({ extended: false }))

// parse application/json
app.use(bodyParser.json({limit: '10mb'}))
app.use(express.static('public'))
app.post('/', (req, res) => {
    try {
        connection.query(`INSERT INTO data VALUES (?) ON DUPLICATE KEY UPDATE \`${DEFAULT_LANG}\` = VALUES(\`${DEFAULT_LANG}\`);`, req.body, (err, response) => {
            if(err) {
                res.json({code: 1, err: err})
            }
            else {
                connection.query('SELECT * FROM data;', (err, response) => {
                    if(err) {
                        res.json({code: 1, err: err})
                    }
                    else {
                        let ret = ''
                        const keys = Object.keys(response[0])
                        ret += keys.join('\t')
                        ret += '\r\n'
                        
                        const types = []
                        const descriptions = []
                        for (let index = 0; index < keys.length; index++) {
                            types.push('string')
                            descriptions.push('')
                        }
                        ret += types.join('\t')
                        ret += '\r\n'
                        ret += descriptions.join('\t')
                        ret += '\r\n'
                        for (let index = 0; index < response.length; index++) {
                            const element = response[index];
                            const values = Object.values(element)
                            ret += values.join('\t')
                            ret += '\r\n'
                        }
                        res.json({code: 0, ret: ret})
                    }
                })
            }
        })
    } 
    catch (error) {
        
    }
})

app.listen(30000, () => {console.log('server start listen 30000')})