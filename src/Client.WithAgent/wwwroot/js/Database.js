const table = "Input"
const version = 1
Database = {
    write: writeData,
    read: readData
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
            const objectStore = 
                db.createObjectStore(table, {keyPath: "session"})
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
        req.onupgradeneeded = e => 
            resolve("")
        req.onsuccess = e => read(e.target.result)
    })
}
