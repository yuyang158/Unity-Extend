const express = require("express")
const path = require("path")
const cookieParser = require("cookie-parser")
const logger = require("morgan")
const fileUpload = require("express-fileupload")
const fileUploadRouter = require("./routes/log-file")
const luaRemoteCmd = require("./routes/lua-remote-cmd")

const cors = require("cors")

const app = express()

app.use(cors())
app.use(logger("dev"))
app.use(express.json())
app.use(express.urlencoded({ extended: false }))
app.use(cookieParser())
// app.use(express.static(path.join(__dirname, "web/dist")))
app.use(express.static(path.join(__dirname, "log")))

app.use(
  fileUpload({
    limits: { fileSize: 50 * 1024 * 1024 },
    useTempFiles: true,
    tempFileDir: "tmp/",
  })
)

app.use((req, res, next) => {
  next()
})

app.use("/file", fileUploadRouter)
app.use(luaRemoteCmd)

app.use((err, req, res, next) => {
  console.error(err.stack)
  res.status(400).send(err.message)
})

console.log("Server started")

module.exports = app
