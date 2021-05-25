const express = require('express');
const fileUpload = require('express-fileupload');
const app = express();
const { spawn } = require('child_process');
const { rename, unlinkSync } = require('fs');


const PORT = 8666;
app.use(fileUpload({
    createParentPath: true,
    useTempFiles : true,
    tempFileDir : '/tmp/'
}));

app.use(express.static('public'));
let processing = {};

app.get('/ping', function(req, res) {
    res.send('pong');
});

app.get('/download', (req, res) => {
    const target = `${__dirname}/public/${req.query.name}-${req.query.radio}.fbx`;
    res.sendFile(target, (err) => {
        unlinkSync(target);
    });
});

app.get('/status', (req, res) => {
    const ret = processing[req.query.name];
    if(ret) {
        res.send(ret);
    }
    else {
        res.send('none');
    }
});

app.post('/upload', function(req, res) {
    if (!req.files || Object.keys(req.files).length === 0) {
        res.status(400).send('No files were uploaded.');
        return;
    }

    const now = Date.now();
    const target = `tmp/${now}.fbx`;
    req.files.file.mv(target, (err) => {
        if(err) {
            res.send(err.message);
        }
        else {
            res.send('success');
        }

        processing[req.query.name] = "start";
        const radio = req.query.radio;
        const blender = spawn('C:\\Program Files\\Blender Foundation\\Blender 2.92\\blender.exe',
            ['-P', '..\\decimate.py', '-b', '-noaudio', '--', radio, target]);

        blender.stdout.on('data', (data) => {
            processing[req.query.name] = data;
            console.log(`stdout: ${data}`);
        });

        blender.stderr.on('data', (data) => {
            processing[req.query.name] = data;
            console.error(`stderr: ${data}`);
        });

        blender.on('close', (code) => {
            console.log(`child process exited with code ${code}`);
            unlinkSync(target);
            rename(`tmp/${now}-export.fbx`, `public/${req.query.name}-${radio}.fbx`, () => {
                delete processing[req.query.name];
            });
        });
    });
});

app.listen(PORT, function() {
    console.log('Express server listening on port ', PORT); // eslint-disable-line
});