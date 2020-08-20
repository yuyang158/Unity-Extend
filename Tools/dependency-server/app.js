const express = require('express');
const path = require('path');
const cookieParser = require('cookie-parser');
const logger = require('morgan');

const mysql = require('mysql');
const sqlPool = mysql.createPool({
    host: '127.0.0.1',
    port: 3306,
    database: 'dependency',
    user: 'root',
    password: '6BpfwPlxw0b8F77A'
});
const indexRouter = require('./routes/index')(sqlPool);
const app = express();

app.use(logger('dev'));
app.use(express.json());
app.use(express.urlencoded({ extended: false }));
app.use(cookieParser());
app.use(express.static(path.join(__dirname, 'public')));

app.use('/', indexRouter);

module.exports = app;
