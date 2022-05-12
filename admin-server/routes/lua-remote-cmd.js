const express = require("express");
const Router = express.Router();
const config = require("config");
const bent = require("bent");
const remoteDebugUrl = config.get("lua_remote_debug_url");
const post = bent(remoteDebugUrl, "POST", "string", 200);
const get = bent(remoteDebugUrl, "GET");

Router.get("/lrc/devices", async (req, res) => {
  const response = await get("?cmd=devices");
  res.json({
    code: 20000,
    content: response
  })
})

Router.post("/lrc/cmd", async (req, res) => {
  const device = req.body.device;

  const response = await post(`?cmd=lua&device=${device}`, {
    body: req.body.lua,
  });
  res.json({
    code: 20000,
    content: response
  })
})

module.exports = Router
