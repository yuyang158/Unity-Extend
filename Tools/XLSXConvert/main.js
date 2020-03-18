const XLSX = require('xlsx')
const fs = require('fs')

let letters = [];
for(var i = 65; i < 91; i++) {
    letters.push(String.fromCharCode(i));
}

let prefixLetters = [''].concat(letters)


const wb = XLSX.readFile( './test_excel.xlsx' )
wb.SheetNames.forEach(sheetName => {
    const sheet = wb.Sheets[sheetName]
    for (const prefix of prefixLetters) {
        for (const letter of object) {
            const key = `${prefix}${letter}2`
            const content = sheet[key]
        }
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