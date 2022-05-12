const bent = require("bent");
const notifyBaseUrl = "";
const notifyUrlPath = "";

module.exports.notify = function (downloadLink, pageLink, mentionList) {
  const post = bent(notifyBaseUrl, "POST", "json");
  const content = `Log uploaded.\n 查看: ${downloadLink}\n 查看其它: ${pageLink}`;
  post(notifyUrlPath, {
    msgtype: "text",
    text: { content, mentioned_list: mentionList },
  });
};
