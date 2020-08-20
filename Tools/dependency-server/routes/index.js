const express = require('express');
const router = express.Router();
const async = require('async');
let sqlPool;

router.get('/', function(req, res, next) {
  res.render('index', { title: 'Express' });
});

router.get('/get-dependency', (req, res) => {
  const project = req.query.project;
  const source = req.query.source;
  sqlPool.query('SELECT `source` FROM ?? WHERE `dep` = ?;', [project, source], (err, results) => {
    if(err) {
      res.json({code: 500, err: err.toString()});
    }
    else {
      res.json({code: 200, results: results});
    }
  });
});

router.post('/upload-dependency', (req, res) => {
  console.log('upload-dependency');
  const project = req.body.project;
  async.waterfall([
    (cb) => {
      sqlPool.query('CREATE TABLE IF NOT EXISTS ?? (\n' +
          '  `source` VARCHAR(100) NOT NULL,\n' +
          '  `dep` VARCHAR(100) NOT NULL,\n' +
          '  PRIMARY KEY (`source`, `dep`));', [project], err => {
        cb(err);
      })
    },
    (cb) => {
      const source = req.body.source;
      const relations = req.body.relations;
      async.each(relations, (relation, callback) => {
        sqlPool.query('DELETE FROM ?? WHERE `source` = ?', [project, source], err => {
          if(err) {
            callback(err);
          }
          else {
            sqlPool.query('INSERT INTO ?? (`source`, `dep`) VALUES (?, ?);', [project, source, relation], callback);
          }
        });
      }, err => {
        cb(err);
      });
    }
  ], err => {
    if(err) {
      res.json({code: 500, err: err.toString()});
    }
    else {
      res.json({code: 200});
    }
  });
});

module.exports = function(pool) {
  sqlPool = pool;
  return router
};
