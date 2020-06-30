const XLSX = require('xlsx');
const fs = require('fs');
const request = require('request');

let letters = [];
for(let i = 65; i < 91; i++) {
    letters.push(String.fromCharCode(i));
}

let prefixLetters = [''].concat(letters);
const collectTranslateText = [];
const MAX_ROW_COUNT = 100000;
let currentProcessKey;

const specialTypeProcess = {
    'int': function(sheet, colPrefix) {
        for (let index = 4; index < MAX_ROW_COUNT; index++) {
            currentProcessKey = `${colPrefix}${index}`;
            if(!sheet[currentProcessKey])
                break;
            parseInt(sheet[currentProcessKey].v)
        }
    },
    'number': function(sheet, colPrefix) {
        for (let index = 4; index < MAX_ROW_COUNT; index++) {
            const currentProcessKey = `${colPrefix}${index}`;
            if(!sheet[currentProcessKey])
                break;
            parseFloat(sheet[currentProcessKey].v)
        }
    },
    'json': function(sheet, colPrefix) {
        for (let index = 4; index < MAX_ROW_COUNT; index++) {
            const currentProcessKey = `${colPrefix}${index}`;
            if(!sheet[currentProcessKey])
                break;
            JSON.parse(sheet[currentProcessKey].v)
        }
    },
    'translate': function(sheet, colPrefix, sheetName) {
        const colName = sheet[`${colPrefix}1`].v;
        for (let index = 4; index < MAX_ROW_COUNT; index++) {
            currentProcessKey = `${colPrefix}${index}`;
            if(!sheet[currentProcessKey])
                break;

            const id = sheet[`A${index}`].v;
            const translateKey = `${sheetName}:${id}:${colName}`;
            collectTranslateText.push([translateKey, sheet[currentProcessKey] ? sheet[currentProcessKey].v : '']);
            
            delete sheet[currentProcessKey]
        }
    }
};

const wb = XLSX.readFile( './test_excel.xlsx' );
wb.SheetNames.forEach(sheetName => {
    if(sheetName.startsWith("ignore")) {
        return;
    }
    const sheet = wb.Sheets[sheetName];
    for (const prefix of prefixLetters) {
        for (const letter of letters) {
            const key = `${prefix}${letter}2`;
            if(!sheet[key])
                break;
            const typValue = sheet[key].v;
            const proc = specialTypeProcess[typValue];
            if(!proc)
                continue;
            try {
                proc(sheet, `${prefix}${letter}`, sheetName)
            }
            catch(err) {
                console.log(`error occurred when procee key : ${currentProcessKey}, error : ${err}`)
            }
        }
    }

    if(collectTranslateText.length > 0) {
        request.post('http://127.0.0.1:30000', {body: collectTranslateText, json: true}, (err, _, res) => {
            if(err) {
                console.log(err);
                return
            }

            fs.writeFileSync('config/i18n.tsv', res.ret)
        })
    }


    let csvData = XLSX.utils.sheet_to_csv(sheet, {FS: '\t', blankrows: false});
    let regex = /""/gi;
    csvData = csvData.replace(regex, '"');
    regex = /\t"/gi;
    csvData = csvData.replace(regex, '\t');
    regex = /"\t/gi;
    csvData = csvData.replace(regex, '\t');
    
    if(!fs.existsSync('./export')) {
        fs.mkdirSync('./export')
    }
    fs.writeFileSync(`./export/${sheetName}.tsv`, csvData)
});