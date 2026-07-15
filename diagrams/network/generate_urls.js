const zlib = require('zlib');
const fs = require('fs');
const path = require('path');

const files = ['class_diagram.puml', 'sequence_diagram.puml'];
const outputDir = 'H:/Star/diagrams/network';

function encodePuml(data) {
    const cleaned = data.replace(/\r/g, '').replace(/\n$/, '');
    const deflated = zlib.deflateRawSync(cleaned);
    // PlantUML base64-like encoding
    const base64 = '0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz-_';
    let result = '';
    for (let i = 0; i < deflated.length; i += 3) {
        const b1 = deflated[i];
        const b2 = i + 1 < deflated.length ? deflated[i + 1] : 0;
        const b3 = i + 2 < deflated.length ? deflated[i + 2] : 0;
        result += base64[b1 >> 2];
        result += base64[((b1 << 4) | (b2 >> 4)) & 63];
        result += base64[((b2 << 2) | (b3 >> 6)) & 63];
        result += base64[b3 & 63];
    }
    return result;
}

for (const file of files) {
    const pumlPath = path.join(outputDir, file);
    if (fs.existsSync(pumlPath)) {
        const data = fs.readFileSync(pumlPath, 'utf8');
        const encoded = encodePuml(data);
        const url = `https://www.plantuml.com/plantuml/svg/${encoded}`;
        console.log(`\n${file}:`);
        console.log(url);
    }
}
