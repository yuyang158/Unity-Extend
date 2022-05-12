const bent = require("bent")
const config = require('config')
const notifyBaseUrl = config.get('im.wechat.notifyBaseUrl')
const notifyUrlPath = config.get('im.wechat.notifyUrlPath')

module.exports.notify = function (downloadLink, pageLink, mentionList) {
  const post = bent(notifyBaseUrl, "POST", "json")
  const content = `Log uploaded.\n 查看: ${downloadLink}\n 查看所有: ${pageLink}`
  post(notifyUrlPath, {
    msgtype: "text",
    text: { content, mentioned_list: mentionList },
  })
}
