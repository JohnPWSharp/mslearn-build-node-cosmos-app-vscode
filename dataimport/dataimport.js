//@ts-check
import {config} from "./config.js";
import * as cosmos from "@azure/cosmos";
import courseData from "./CourseData.json";

const client = new cosmos.CosmosClient(config.connectionString);
const databaseid = config.database;
const containerid = config.container;
const partitionKey = { kind: "Hash", paths: ["/AcademicYear"] };
const throughput = 1000;

function isOK(statusCode) {
    return statusCode >= 200 && statusCode <= 299;
}

async function asyncForEach(array, callback) {
    for (let index = 0; index < array.length; index++) {
      await callback(array[index], index, array);
    }
}

async function uploadDocument(value, index, array) {
    process.stdout.write(`Adding document with id: ${value.id}\n`);

    const containerref = client.database(databaseid).container(containerid);

    await containerref.items.create(value)
        .then(({item, statusCode}) => {
            isOK(statusCode) && process.stdout.write(`Added document with id: ${item.id}\n`);
        })
        .catch((err) => { process.stdout.write(`Failed to add document with id: ${value.id}\n${err.message}`);
        });
}

async function setupContainer() {
    const containerref = client.database(databaseid).container(containerid);
    await containerref.delete().then(() => {
        process.stdout.write("Existing container removed\n");
    }).catch(() => {
        // Ignore errors if container doesn't exist
    }).then(() => {
        client.database(databaseid).containers.create({id: containerid, partitionKey}, {offerThroughput: throughput});
    });
}

async function verifyContainer() {
    await client.database(databaseid).container(containerid).read()
        .then(() => {
            process.stdout.write("Created new container\n");
    });
}

(async () => {
    await setupContainer();
    setTimeout(() => {
        verifyContainer();
        asyncForEach(courseData, uploadDocument);
    }, 5000);
})();