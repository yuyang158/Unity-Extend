const express = require('express');
const path = require('path');
const cookieParser = require('cookie-parser');
const logger = require('morgan');
const fileUpload = require('express-fileupload');
const fileUploadRouter = require('./routes/log-file');
const cors = require('cors');

const app = express();

app.use(cors());
app.use(logger('dev'));
app.use(express.json());
app.use(express.urlencoded({ extended: false }));
app.use(cookieParser());
app.use(express.static(path.join(__dirname, 'public')));
app.use(express.static(path.join(__dirname, 'log')));

app.use(fileUpload({
    limits: {fileSize: 50 * 1024 * 1024},
    useTempFiles: true,
    tempFileDir: 'tmp/'
}));

app.use('/file', fileUploadRouter);

module.exports = app;
