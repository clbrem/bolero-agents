 const table = "Input"
const logs= "Logs"
const session = "session"
const version = 1
Database = {
    write: writeData,
    read: readData,
    log: log,
    readLog: readLog
}
function writeData(data) {
    return new Promise((resolve,reject) => {
        const write= (db) => {
            const trans = db.transaction(table, "readwrite")
            const objectStore = trans.objectStore(table)
            trans.onerror = reject
            trans.oncomplete = () => db.close()
            const req = objectStore.put(data)
            req.onsuccess = e => resolve(e.target.result)
            req.onerror = reject
        }
        const req = indexedDB.open(table, version)
        req.onerror = reject
        req.onupgradeneeded = e => {
            const db = e.target.result
            db.createObjectStore(table, {keyPath: session})
        }
        req.onsuccess = e => write(e.target.result)
    })
}

function readData(data) {
    return new Promise((resolve,reject) => {
        const read= (db) => {
            const req = 
                db.transaction(table)
                    .objectStore(table)
                    .get(data)
            req.onsuccess = _ => resolve(req.result.input)
            req.onerror = reject            
        }
        const req = indexedDB.open(table, version)
        req.onerror = reject
        req.onupgradeneeded = _ => 
            resolve("")
        req.onsuccess = e => read(e.target.result)
    })
}

function log(data) {
    return new Promise((resolve, reject) => {
        const write = (db) => {
            const trans = db.transaction(logs, "readwrite")
            const objectStore = trans.objectStore(logs)
            trans.onerror = reject
            trans.oncomplete = () => db.close()
            const req = objectStore.add(data)
            req.onsuccess = e => resolve(e.target.result)
            req.onerror = reject            
        }
        const req = indexedDB.open(logs, version)
        req.onerrr = reject
        req.onupgradeneeded = e => {
            const db = e.target.result
            const objectStore = 
                db.createObjectStore(logs, {autoIncrement: true});
            objectStore.createIndex(session, session, {unique: false});
        }
        req.onsuccess = e => write(e.target.result) 
    })
}
function readLog(session) {
    return new Promise((resolve, reject) => {
        const read = (db) => {
            let response = []
            const index = 
                db.transaction(logs).objectStore(logs).index(session)
            const cursor = index.openCursor()
            cursor.onerror = reject
            cursor.onsuccess = event => {
                const curs= event.target.result
                if (curs) {
                    curs.push(curs.value.message)
                } 
            }
            return resolve(response)
        }
        const req = indexedDB.open(logs, version)
        req.onerror = reject
        req.onupgradeneeded = _ => resolve([])
        req.onsuccess = e => read(e.target.result)
    }) 
}
