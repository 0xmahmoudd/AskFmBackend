#!/bin/bash

# Ensure colors
GREEN='\033[0;32m'
RED='\033[0;31m'
NC='\033[0m' # No Color

echo "====================================================="
echo "  AskFm Performance Stress Test Script"
echo "====================================================="
echo ""
echo "1. Checking if API is running..."

# Wait for API to be responsive
API_URL="http://localhost:5180"
if ! curl -s $API_URL/swagger/index.html > /dev/null; then
    echo -e "${RED}Error: API is not running on $API_URL.${NC}"
    echo "Please run 'dotnet run --project AskFm.API' in another terminal first."
    exit 1
fi

echo -e "${GREEN}API is running!${NC}"
echo ""

echo "2. Seeding database with 10,000 dummy users, 1 thread, 10,000 likes, and 5,000 comments..."
echo "This may take 10-30 seconds..."

SEED_RESPONSE=$(curl -s -X POST $API_URL/api/Seed/stress-test)
USER_ID=$(echo $SEED_RESPONSE | grep -o '"userId":[0-9]*' | awk -F':' '{print $2}')
THREAD_ID=$(echo $SEED_RESPONSE | grep -o '"threadId":[0-9]*' | awk -F':' '{print $2}')

if [ -z "$USER_ID" ]; then
    echo -e "${RED}Failed to seed data. Response was:${NC}"
    echo $SEED_RESPONSE
    exit 1
fi

echo -e "${GREEN}Data seeded successfully! Test User ID: $USER_ID, Thread ID: $THREAD_ID${NC}"
echo ""

echo "3. Measuring 'GetFeed' performance before enhancement..."
TOTAL_TIME=0
for i in {1..3}
do
    RESPONSE=$(curl -s -X GET "$API_URL/api/Seed/test-feed?userId=$USER_ID")
    TIME_TAKEN=$(echo $RESPONSE | grep -o '"timeTakenMs":[0-9]*' | awk -F':' '{print $2}')
    echo "Feed Attempt $i: ${TIME_TAKEN}ms"
    TOTAL_TIME=$((TOTAL_TIME + TIME_TAKEN))
done
AVG_TIME_FEED=$((TOTAL_TIME / 3))

echo ""
echo "4. Measuring 'GetThreadById' performance before enhancement..."
TOTAL_TIME=0
for i in {1..3}
do
    RESPONSE=$(curl -s -X GET "$API_URL/api/Seed/test-thread?threadId=$THREAD_ID")
    TIME_TAKEN=$(echo $RESPONSE | grep -o '"timeTakenMs":[0-9]*' | awk -F':' '{print $2}')
    echo "Thread Attempt $i: ${TIME_TAKEN}ms"
    TOTAL_TIME=$((TOTAL_TIME + TIME_TAKEN))
done
AVG_TIME_THREAD=$((TOTAL_TIME / 3))

echo ""
echo "5. Measuring 'GetUserNotifications' performance before enhancement..."
TOTAL_TIME=0
for i in {1..3}
do
    RESPONSE=$(curl -s -X GET "$API_URL/api/Seed/test-notifications?userId=$USER_ID")
    TIME_TAKEN=$(echo $RESPONSE | grep -o '"timeTakenMs":[0-9]*' | awk -F':' '{print $2}')
    echo "Notifications Attempt $i: ${TIME_TAKEN}ms"
    TOTAL_TIME=$((TOTAL_TIME + TIME_TAKEN))
done
AVG_TIME_NOTIF=$((TOTAL_TIME / 3))

echo ""
echo -e "${RED}>>> AVERAGE TIME FOR FEED: ${AVG_TIME_FEED}ms <<<${NC}"
echo -e "${RED}>>> AVERAGE TIME FOR THREAD: ${AVG_TIME_THREAD}ms <<<${NC}"
echo -e "${RED}>>> AVERAGE TIME FOR NOTIFICATIONS: ${AVG_TIME_NOTIF}ms <<<${NC}"
echo ""
echo "====================================================="
