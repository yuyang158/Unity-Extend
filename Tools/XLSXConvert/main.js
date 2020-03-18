const XLSX = require('xlsx')
const fs = require('fs')
const request = require('request')

let letters = [];
for(var i = 65; i < 91; i++) {
    letters.push(String.fromCharCode(i));
}

let prefixLetters = [''].concat(letters)
const collectTranslateText = []
const MAX_ROW_COUNT = 100000

const specialTypeProcess = {
    'int': function(sheet, colPrefix) {
        for (let index = 4; index < MAX_ROW_COUNT; index++) {
            const key = `${colPrefix}${index}`
            if(!sheet[key])
                break
            parseInt(sheet[key].v)
        }
    },
    'number': function(sheet, colPrefix) {
        for (let index = 4; index < MAX_ROW_COUNT; index++) {
            const key = `${colPrefix}${index}`
            if(!sheet[key])
                break
            parseFloat(sheet[key].v)
        }
    },
    'json': function(sheet, colPrefix) {
        for (let index = 4; index < MAX_ROW_COUNT; index++) {
            const key = `${colPrefix}${index}`
            if(!sheet[key])
                break
            JSON.parse(sheet[key].v)
        }
    },
    'translate': function(sheet, colPrefix, sheetName) {
        const colName = sheet[`${colPrefix}1`].v
        for (let index = 1; index < MAX_ROW_COUNT; index++) {
            const delKey = `${colPrefix}${index}`
            if(!sheet[delKey])
                break

            if(index > 3) {
                const id = sheet[`A${index}`].v
                const translateKey = `${sheetName}:${id}:${colName}`
                collectTranslateText.push([translateKey, sheet[delKey] ? sheet[delKey].v : ''])
            }
            
            delete sheet[delKey]
        }
    }
}

const wb = XLSX.readFile( './test_excel.xlsx' )
wb.SheetNames.forEach(sheetName => {
    const sheet = wb.Sheets[sheetName]
    for (const prefix of prefixLetters) {
        for (const letter of letters) {
            const key = `${prefix}${letter}2`
            if(!sheet[key])
                break
            const typValue = sheet[key].v
            const proc = specialTypeProcess[typValue]
            if(!proc)
                continue
            proc(sheet, `${prefix}${letter}`, sheetName)
        }
    }

    if(collectTranslateText.length > 0) {
        request.post('http://127.0.0.1:30000', {body: collectTranslateText, json: true}, (err, _, res) => {
            if(err) {
                console.log(err)
                return
            }

            fs.writeFileSync('config/i18n.tsv', res.ret)
        })
    }


    let csvData = XLSX.utils.sheet_to_csv(sheet, {FS: '\t', blankrows: false})
    let regex = /""/gi
    csvData = csvData.replace(regex, '"')
    regex = /\t"/gi
    csvData = csvData.replace(regex, '\t')
    regex = /"\t/gi
    csvData = csvData.replace(regex, '\t')
    
    if(!fs.existsSync('./config')) {
        fs.mkdirSync('./config')
    }
    fs.writeFileSync(`./config/${sheetName}.tsv`, csvData)
})